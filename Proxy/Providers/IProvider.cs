using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Loye.Proxy
{
    public interface IProvider
    {
        IPEndPoint GetRemoteEndPoint(string host, int port);

    }
}
