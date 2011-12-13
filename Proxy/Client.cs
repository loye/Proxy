using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Loye.Proxy
{
    public abstract class Client : IClient
    {
        public Socket ClientSocket { get; set; }

        public Socket RemoteSocket { get; set; }

        public Action<IClient> Destroyer { get; set; }

        public DateTime ExpireTime { get; private set; }

        protected byte[] ClientBuffer;

        protected byte[] RemoteBuffer;

        public Client()
        {
            ClientBuffer = new byte[ListenerConfig.CLIENT_BUFFER_SIZE];
            RemoteBuffer = new byte[ListenerConfig.REMOTE_BUFFER_SIZE];
            ExpireTime = DateTime.Now.AddSeconds(ListenerConfig.TIME_OUT_SECONDS);
        }

        public abstract void StartHandshake();

        public void StartRelay()
        {
            try
            {
                ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, this.OnClientReceive, ClientSocket);
                RemoteSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, this.OnRemoteReceive, RemoteSocket);
            }
            catch (Exception ex)
            {
                Dispose();
                Helper.PublishException(ex);
            }
        }

        // Client Cycle
        private void OnClientReceive(IAsyncResult ar)
        {
            try
            {
                if (ClientSocket.Connected)
                {
                    int length = ClientSocket.EndReceive(ar);
                    if (length > 0 && RemoteSocket.Connected)
                    {
                        RemoteSocket.BeginSend(ClientBuffer, 0, length, SocketFlags.None, new AsyncCallback(this.OnRemoteSent), RemoteSocket);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                Helper.PublishException(ex);
            }
            Dispose();
        }

        // Client Cycle
        private void OnRemoteSent(IAsyncResult ar)
        {
            try
            {
                if (RemoteSocket.Connected)
                {
                    int length = RemoteSocket.EndSend(ar);
                    if (length > 0 && ClientSocket.Connected)
                    {
                        ClientSocket.BeginReceive(ClientBuffer, 0, ClientBuffer.Length, SocketFlags.None, this.OnClientReceive, ClientSocket);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                Helper.PublishException(ex);
            }
            Dispose();
        }

        // Remote Cycle
        private void OnRemoteReceive(IAsyncResult ar)
        {
            try
            {
                if (RemoteSocket.Connected)
                {
                    int length = RemoteSocket.EndReceive(ar);
                    if (length > 0 && ClientSocket.Connected)
                    {
                        if (Encoding.ASCII.GetString(RemoteBuffer, 0, 4).StartsWith("HTTP"))
                        {
                            string respose = Encoding.ASCII.GetString(RemoteBuffer, 0, length);
                            int sp = respose.IndexOf("\r\n");
                            Helper.Debug(respose.Substring(0, sp > 0 ? sp + 2 : 1024), ConsoleColor.DarkYellow);
                        }
                        ClientSocket.BeginSend(RemoteBuffer, 0, length, SocketFlags.None, this.OnClientSent, ClientSocket);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                Helper.PublishException(ex);
            }
            Dispose();
        }

        // Remote Cycle
        private void OnClientSent(IAsyncResult ar)
        {
            try
            {
                if (ClientSocket.Connected)
                {
                    int length = ClientSocket.EndSend(ar);
                    if (length > 0 && RemoteSocket.Connected)
                    {
                        RemoteSocket.BeginReceive(RemoteBuffer, 0, RemoteBuffer.Length, SocketFlags.None, this.OnRemoteReceive, RemoteSocket);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose();
                Helper.PublishException(ex);
            }
            Dispose();
        }

        private void ReleaseSocket(Socket socket)
        {
            if (socket != null && socket.Connected)
            {
                try
                {
                    socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception ex)
                {
                    Helper.PublishException(ex);
                }
                socket.Close();
                socket.Dispose();
                socket = null;
            }
        }

        public void Dispose()
        {
            ReleaseSocket(ClientSocket);
            ReleaseSocket(RemoteSocket);
            if (this.Destroyer != null)
            {
                this.Destroyer(this);
            }
        }
    }
}
