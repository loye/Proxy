using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Loye.Proxy
{
    public abstract class Listener<TClient> : IListener
        where TClient : IClient, new()
    {
        #region Private fields

        private readonly ListenerType _listenerType;

        private readonly IPEndPoint _listenerEndPoint;

        private readonly string _host;

        private readonly int _port;

        private Socket _listenSocket;

        private SynchronizedCollection<IClient> _clients;

        #endregion


        public IProvider Provider { get; set; }


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

            _listenerType = type;
            _listenerEndPoint = new IPEndPoint(address, port);
            _host = host;
            _port = port;
            _clients = new SynchronizedCollection<IClient>();
        }

        public void Start()
        {
            _listenSocket = new Socket(_listenerEndPoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(_listenerEndPoint);
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

        protected void OnAccept(IAsyncResult ar)
        {
            Socket acceptedSocket = null;
            try
            {
                _listenSocket.BeginAccept(this.OnAccept, _listenSocket);
                acceptedSocket = _listenSocket.EndAccept(ar);
            }
            catch (Exception ex)
            {
                DebugHelper.PublishException(ex);
            }

            TClient client = new TClient();
            client.ClientSocket = acceptedSocket;
            client.Destroyer = c => _clients.Remove(c);
            client.Provider = this.Provider;
            _clients.Add(client);
            client.Start();
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
            return string.Format("{0} [{1}:{2}] {3}",
                this._listenerType,
                this._host,
                this._port,
                this.Provider.GetType().Name);
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
            DebugHelper.Debug(string.Format("Recycled clients: {0}; Clients count: {1}", count, _clients.Count));
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
                    DebugHelper.PublishException(ex);
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
