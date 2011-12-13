using System.Collections.Generic;
using System.Xml.Serialization;

namespace Loye.Proxy.Configuration
{
    #region root

    [XmlRoot(ElementName = "configuration", Namespace = GeneralConfig.NAMESPACE)]
    public class ConfigurationRoot
    {
        [XmlElement(ElementName = "listeners")]
        public Listeners Listeners { get; set; }

        [XmlElement(ElementName = "proxies")]
        public Proxies Proxies { get; set; }

        [XmlElement(ElementName = "providers")]
        public Providers Providers { get; set; }

        [XmlElement(ElementName = "dns.lookup")]
        public DnsLookup DnsLookup { get; set; }
    }

    #endregion

    #region listener

    public class Listeners
    {
        [XmlElement(ElementName = "listener")]
        public List<ListenerItem> ListenerList { get; set; }
    }

    public class ListenerItem
    {
        [XmlAttribute(AttributeName = "type")]
        public ListenerType Type { get; set; }

        [XmlAttribute(AttributeName = "host")]
        public string Host { get; set; }

        [XmlAttribute(AttributeName = "port")]
        public int Port { get; set; }

        [XmlAttribute(AttributeName = "proxyName")]
        public string ProxyName { get; set; }

        [XmlAttribute(AttributeName = "providerName")]
        public string ProviderName { get; set; }
    }

    #endregion

    #region proxy

    public class Proxies
    {
        [XmlElement(ElementName = "proxy")]
        public List<ProxyItem> ProxyList { get; set; }
    }

    public class ProxyItem
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "host")]
        public string Host { get; set; }

        [XmlAttribute(AttributeName = "port")]
        public int Port { get; set; }
    }

    #endregion

    #region provider

    public class Providers
    {
        [XmlElement(ElementName = "provider")]
        public List<ProviderItem> ProviderList { get; set; }
    }

    public class ProviderItem
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "url")]
        public string Url { get; set; }

        [XmlElement(ElementName = "parameter")]
        public List<Parameter> ParameterList { get; set; }
    }

    public class Parameter
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    #endregion

    #region dns

    public class DnsLookup
    {
        [XmlElement(ElementName = "item")]
        public List<DnsItem> DnsList { get; set; }
    }

    public class DnsItem
    {
        [XmlAttribute(AttributeName = "host")]
        public string Host { get; set; }

        [XmlAttribute(AttributeName = "ip")]
        public string Ip { get; set; }
    }

    #endregion
}
