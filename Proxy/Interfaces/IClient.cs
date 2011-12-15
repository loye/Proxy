using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Loye.Proxy
{
    public interface IClient : IDisposable
    {
        Socket ClientSocket { get; set; }

        Socket RemoteSocket { get; set; }

        Action<IClient> Destroyer { get; set; }

        DateTime ExpireTime { get; }

        void StartHandshake();
    }
}
