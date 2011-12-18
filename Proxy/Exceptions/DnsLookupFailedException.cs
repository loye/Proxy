using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Loye.Proxy
{
    public class DnsLookupFailedException : Exception
    {
        public DnsLookupFailedException(string host)
            : base(string.Format("Host:{0} can't find.", host)) { }
    }
}
