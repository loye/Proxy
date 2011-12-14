using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace Loye.Proxy
{
    public interface IListener : IDisposable
    {
        #region Properties

        IProvider Provider { get; set; }

        IPEndPoint ProxyEndPoint { get; set; }

        #endregion

        #region Methods

        void Start();

        void Stop();

        void Restart();

        void RecycleClients();

        string GetClientsDebugString();

        #endregion
    }
}
