using System.Net;

namespace Loye.Proxy
{
    public class HttpListener : Listener<HttpClient>
    {
        public HttpListener(string host, int port) : base(ListenerType.Http, host, port) { }
    }
}
