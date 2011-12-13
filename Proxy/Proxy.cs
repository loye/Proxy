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
                    Listeners.Add(CreateListener(l.Type, l.Host, l.Port));
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

        private static IListener CreateListener(ListenerType type, string host, int port)
        {
            switch (type)
            {
                case ListenerType.Http:
                    return new HttpListener(host, port);
                case ListenerType.Sock:
                    return null;
                default:
                    return new HttpListener(host, port);
            }
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
