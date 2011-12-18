using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
namespace Loye.Proxy
{
    internal class DirectAccessProvider : IProvider
    {
        public IPEndPoint _proxyEndPoint;

        public DirectAccessProvider(IPEndPoint proxy)
        {
            _proxyEndPoint = proxy;
        }

        public IPEndPoint GetRemoteEndPoint(string host, int port)
        {
            IPEndPoint remoteEndPoint;
            if (this._proxyEndPoint != null)
            {
                remoteEndPoint = this._proxyEndPoint;
            }
            else
            {
                IPAddress address = DnsCache.GetIPAddress(host);
                if (address == null)
                {
                    throw new DnsLookupFailedException(host);
                }
                remoteEndPoint = new IPEndPoint(address, port);
            }

            return remoteEndPoint;
        }
    }
}
