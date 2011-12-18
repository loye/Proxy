using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Loye.Proxy
{
    public class HttpRequest
    {
        private StringBuilder _source;

        public string RequestType { get; set; } // GET|HEAD|POST|PUT|DELETE|TRACE  CONNECT

        public string ProtocolType { get; set; } // HTTP|HTTPS

        public string Url { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Content { get; set; }


        public HttpRequest()
        {
            _source = new StringBuilder();
        }


        public void Append(string s)
        {
            _source.Append(s);
        }


        public override string ToString()
        {
            return _source.ToString();
        }

    }
}
