
namespace Loye.Proxy
{
    internal static class GeneralConfig
    {
        internal const string CONFIG_XML_PATH = ".\\config.xml";
        internal const string NAMESPACE = "http://configuration.proxy.loye";
    }

    internal static class ListenerConfig
    {
        internal const ListenerType DEFAULT_TYPE = ListenerType.Http;
        internal const string DEFAULT_HOST = "127.0.0.1";
        internal const int DEFAULT_PORT = 8008;
        internal const int DEFAULT_BACKLOG = 100;
        internal const int CLIENT_BUFFER_SIZE = 8192;
        internal const int REMOTE_BUFFER_SIZE = 8192;
        internal const int TIME_OUT_SECONDS = 300;
    }

    internal static class ErrorPages
    {
        internal const string HTTPS_CONNECTED = "{0} 200 Connection established\r\nProxy-Agent: Loye's Proxy Server\r\n\r\n";
        internal const string DNS_LOOKUP_FAILED = "HTTP/1.1 502 DNS Lookup Failed\r\nContent-Type: text/html; charset=UTF-8\r\nConnection: close\r\n\r\nLoye's Proxy: DNS Lookup for {0} failed. No such host is known\r\n";
        internal const string BAD_REQUEST = "HTTP/1.1 400 Bad Request\r\nContent-Type: text/html; charset=UTF-8\r\nConnection: close\r\n\r\nLoye's Proxy: Bad Request\r\n";

    }

    internal static class DnsConfig
    {
        internal const int EXPIRE_MINUTES = 120;
    }
}
