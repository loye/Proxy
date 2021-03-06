﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Loye.Proxy
{
    public abstract class Listener<TClient> : IListener
        where TClient : IClient, new()
    {
        #region Private fields

        private readonly ListenerType _listenerType;

        private readonly IPAddress _address;

        private readonly string _host;

        private readonly int _port;

        private readonly IProvider _provider;

        


        private Socket _listenSocket;

        private SynchronizedCollection<IClient> _clients;

        #endregion

        public Listener(ListenerType type, string host, int port)
        {
            IPAddress address;
            if (!IPAddress.TryParse(host, out address))
            {
                address = DnsCache.GetIPAddress(host);
            }
            if (address == null)
            {
                throw new ArgumentException("host " + host + " not found.", "host");
            }
            if (port < 1 || port > 65535)
            {
                throw new ArgumentOutOfRangeException("port[" + port + "] out of range.", "port");
            }

            this._listenerType = type;
            this._address = address;
            this._host = host;
            this._port = port;
            this._clients = new SynchronizedCollection<IClient>();
        }

        public void Start()
        {
            _listenSocket = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(new IPEndPoint(_address, _port));
            _listenSocket.Listen(ListenerConfig.DEFAULT_BACKLOG);
            _listenSocket.BeginAccept(this.OnAccept, _listenSocket);
        }

        public void Stop()
        {
            _listenSocket.Dispose();
            //this.Dispose();
        }

        public void Restart()
        {
            this.Stop();
            this.Start();
        }

        private void OnAccept(IAsyncResult ar)
        {
            Socket acceptedSocket = null;
            try
            {
                acceptedSocket = _listenSocket.EndAccept(ar);
                // Begin accept next connection
                _listenSocket.BeginAccept(this.OnAccept, _listenSocket);
            }
            catch (Exception ex)
            {
                Helper.PublishException(ex);
            }

            TClient client = new TClient();
            client.ClientSocket = acceptedSocket;
            client.Destroyer = c => _clients.Remove(c);
            _clients.Add(client);
            client.StartHandshake();
        }

        public string GetClientsDebugString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("Clients Count: {0}\n", _clients.Count));
            foreach (var c in _clients)
            {
                sb.Append(string.Format("[{0}]; Client: {1}; ",
                    c.ExpireTime.ToString("HH:mm:ss"),
                    c.ClientSocket == null
                        ? "null"
                        : c.ClientSocket.Connected.ToString() + " " + (c.ClientSocket.Connected ? c.ClientSocket.RemoteEndPoint.ToString() : "")));
                sb.Append(string.Format("Remote: {0}\n",
                    c.RemoteSocket == null
                        ? "null"
                        : c.RemoteSocket.Connected.ToString() + " " + (c.RemoteSocket.Connected ? c.RemoteSocket.RemoteEndPoint.ToString() : "")));
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}:{2}]", this._listenerType, this._host, this._port);
        }

        public void RecycleClients()
        {
            int count = 0;
            foreach (var c in _clients)
            {
                if (c.ExpireTime < DateTime.Now)
                {
                    c.Dispose();
                    count++;
                }
            }
            Helper.Debug(string.Format("Recycled clients: {0}; Clients count: {1}", count, _clients.Count));
        }

        public void Dispose()
        {
            if (_listenSocket != null)
            {
                try
                {
                    _listenSocket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    Helper.PublishException(ex);
                }
                _listenSocket.Close();
                _listenSocket = null;
            }
            if (_clients != null && _clients.Count > 0)
            {
                foreach (var c in _clients)
                {
                    c.Dispose();
                }
            }
        }
    }
}
