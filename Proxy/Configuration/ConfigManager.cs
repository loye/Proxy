using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;

namespace Loye.Proxy.Configuration
{
    internal static class ConfigManager
    {
        public static void FillDefaultConfiguration()
        {
            // ConfigurationRoot
            if (Proxy.Configuration == null)
            {
                Proxy.Configuration = new ConfigurationRoot();
            }

            // Listeners
            if (Proxy.Configuration.Listeners == null)
            {
                Proxy.Configuration.Listeners = new Listeners();
            }
            if (Proxy.Configuration.Listeners.ListenerList == null)
            {
                Proxy.Configuration.Listeners.ListenerList = new List<ListenerItem>();
            }
            if (Proxy.Configuration.Listeners.ListenerList.Count == 0)
            {
                Proxy.Configuration.Listeners.ListenerList
                    .Add(new ListenerItem()
                    {
                        Type = ListenerConfig.DEFAULT_TYPE,
                        Host = ListenerConfig.DEFAULT_HOST,
                        Port = ListenerConfig.DEFAULT_PORT,
                    });
            }

            // DnsLookup
            if (Proxy.Configuration.DnsLookup == null)
            {
                Proxy.Configuration.DnsLookup = new DnsLookup();
            }
            if (Proxy.Configuration.DnsLookup.DnsList == null)
            {
                Proxy.Configuration.DnsLookup.DnsList = new List<DnsItem>();
            }
            if (Proxy.Configuration.DnsLookup.DnsList.Count == 0)
            {
                Proxy.Configuration.DnsLookup.DnsList
                    .Add(new DnsItem()
                    {
                        Host = "localhost",
                        Ip = "127.0.0.1"
                    });
            }
        }

        public static void LoadConfiguration()
        {
            if (File.Exists(GeneralConfig.CONFIG_XML_PATH))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationRoot));
                using (FileStream fileStream = File.OpenRead(GeneralConfig.CONFIG_XML_PATH))
                {
                    Proxy.Configuration = serializer.Deserialize(fileStream) as ConfigurationRoot;
                }
            }
            FillDefaultConfiguration();
            ImportDns(); //import dns lookup to dns cache
        }

        public static void SaveConfiguration()
        {
            if (File.Exists(GeneralConfig.CONFIG_XML_PATH))
            {
                File.Delete(GeneralConfig.CONFIG_XML_PATH);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationRoot));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, GeneralConfig.NAMESPACE);
            using (FileStream fileStream = File.OpenWrite(GeneralConfig.CONFIG_XML_PATH))
            {
                serializer.Serialize(fileStream, Proxy.Configuration, ns);
            }
        }

        #region Dns Configuration

        public static void ExportDns()
        {
            //Proxy.Configuration.DnsConfig = new DnsNode()
            //    {
            //        ExpireMinutes = DnsCache.ExpireMinutes,
            //        DnsItemList = new List<DnsItem>(),
            //    };
            //DnsCache.GetDnsList().ForEach(d =>
            //{
            //    Proxy.Configuration.DnsConfig.DnsItemList.Add(new DnsItem()
            //    {
            //        Host = d.Key,
            //        Ip = d.Value.ToString(),
            //    });
            //});
        }

        public static void ImportDns()
        {
            var dnsLookup = Proxy.Configuration.DnsLookup;
            if (dnsLookup != null && dnsLookup.DnsList != null)
            {
                var dnsList = new List<KeyValuePair<string, IPAddress>>();
                dnsLookup.DnsList.ForEach(d =>
                {
                    IPAddress address;
                    if (!string.IsNullOrEmpty(d.Host) && IPAddress.TryParse(d.Ip, out address))
                    {
                        dnsList.Add(new KeyValuePair<string, IPAddress>(d.Host, address));
                    }
                });
                DnsCache.AppendDnsList(dnsList, true); // never expire
            }
        }

        #endregion
    }
}
