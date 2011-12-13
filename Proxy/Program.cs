using System;
using Loye.Proxy.Configuration;
using System.Net.Sockets;

namespace Loye.Proxy
{
    class Program
    {
        static void Main(string[] args)
        {
            Proxy.Start();

            Debug();

        }

        static void Debug()
        {
            while (true)
            {
                var input = Console.ReadKey();
                Console.WriteLine();
                if (input.Key == ConsoleKey.E)
                {
                    Proxy.Stop();
                    return;
                }
                else if (input.Key == ConsoleKey.H)
                {
                    Console.WriteLine("H:Help; E:Exit; D:Dns; C:Clients; S:SaveConfig; I:ImportConfig");
                }
                else if (input.Key == ConsoleKey.D)
                {
                    Console.WriteLine(DnsCache.ToDebugString());
                }
                else if (input.Key == ConsoleKey.C)
                {
                    Console.WriteLine(Proxy.Listeners[0].GetClientsDebugString());
                }
                else if (input.Key == ConsoleKey.S)
                {
                    ConfigManager.SaveConfiguration();
                    Console.WriteLine("config.xml saved");
                }
                else if (input.Key == ConsoleKey.I)
                {
                    ConfigManager.LoadConfiguration();
                    Console.WriteLine("config.xml imported");
                }
                else if (input.Key == ConsoleKey.Delete)
                {
                    DnsCache.Clear();
                    Console.WriteLine("Cache cleared");
                }
                else if (input.Key == ConsoleKey.Home)
                {
                    Proxy.Start();
                }
                else if (input.Key == ConsoleKey.End)
                {
                    Proxy.Stop();
                }
            }
        }

    }
}
