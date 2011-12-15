using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Loye.Proxy.Configuration;
using System.Net;

namespace Loye.Proxy
{
    public static class Factory
    {
        public static IListener CreateListener(ListenerItem listenerItem, ConfigurationRoot configuration)
        {
            IListener listener;
            switch (listenerItem.Type)
            {
                case ListenerType.Http:
                default:
                    listener = new HttpListener(listenerItem.Host, listenerItem.Port);
                    break;
            }

            var providerConfig =
                listenerItem.Provider
                ?? configuration.Providers.ProviderList.Find(p => p.Name == listenerItem.ProviderName);
            var proxyConfig =
                listenerItem.Proxy
                ?? configuration.Proxies.ProxyList.Find(p => p.Name == listenerItem.ProxyName);

            listener.Provider = Factory.CreateProvider(providerConfig);
            listener.ProxyEndPoint =
                proxyConfig != null
                ? new IPEndPoint(DnsCache.GetIPAddress(proxyConfig.Host), proxyConfig.Port)
                : null;

            return listener;
        }

        public static IProvider CreateProvider(ProviderItem providerConfig)
        {
            IProvider provider = null;

            if (providerConfig != null)
            {
                switch (providerConfig.Type)
                {
                    case "GoogleEngine":
                        provider = new GoogleEngineProvider();
                        break;
                    case "Direct":
                    default:
                        break;
                }
            }

            if (provider == null)
            {
                provider = new DirectAccessProvider();
            }

            return provider;
        }


    }
}
