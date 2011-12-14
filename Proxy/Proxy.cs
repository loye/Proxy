using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Loye.Proxy.Configuration;
using System.Threading.Tasks;

namespace Loye.Proxy
{
    public static class Proxy
    {
        public static ConfigurationRoot Configuration { get; set; }

        public static List<IListener> Listeners { get; private set; }

        public static Thread RecycleThread { get; private set; }

        static Proxy()
        {
            Listeners = new List<IListener>();

            new Thread(Helper.Print).Start();

            // Load configuration
            ConfigManager.LoadConfiguration();

            Configuration.Listeners.ListenerList
                .ForEach(l =>
                {
                    Listeners.Add(CreateListener(l));
                });

            RecycleThread = new Thread(Recycle);
        }

        public static void Start()
        {
            Listeners.ForEach(l =>
                {
                    l.Start();
                    Helper.Debug("Start listen: " + l.ToString());
                }
            );
            RecycleThread.Start();
        }

        public static void Stop()
        {
            Listeners.ForEach(l =>
                {
                    l.Stop();
                    Helper.Debug("Stop listen: " + l.ToString());
                }
            );
        }

        private static IListener CreateListener(ListenerItem listenerItem)
        {
            IListener listener;
            switch (listenerItem.Type)
            {
                case ListenerType.Http:
                default:
                    listener = new HttpListener(listenerItem.Host, listenerItem.Port);
                    break;
            }
            var provider =
                listenerItem.Provider
                ?? Configuration.Providers.ProviderList.Find(p => p.Name == listenerItem.ProviderName);
            var proxy =
                listenerItem.Proxy
                ?? Configuration.Proxies.ProxyList.Find(p => p.Name == listenerItem.ProxyName);
            //TODO
            return listener;
        }

        private static void Recycle()
        {
            while (true)
            {
                Thread.Sleep(TimeSpan.FromSeconds(30));
                Listeners.ForEach(l =>
                {
                    l.RecycleClients();
                });
            }
        }
    }
}
