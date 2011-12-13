using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Loye.Proxy
{
    internal static class Helper
    {
        private static ConcurrentQueue<KeyValuePair<ConsoleColor, string>> _printerQueue = new ConcurrentQueue<KeyValuePair<ConsoleColor, string>>();

        private static readonly ConsoleColor DEFAULT_FOREGROUND_COLOR = Console.ForegroundColor;

        public static void Debug(string message, ConsoleColor? color = null)
        {
            _printerQueue.Enqueue(new KeyValuePair<ConsoleColor, string>(color ?? DEFAULT_FOREGROUND_COLOR, message));
        }

        public static void Print()
        {
            KeyValuePair<ConsoleColor, string> item;
            while (true)
            {
                if (_printerQueue.TryDequeue(out item))
                {
                    Console.ForegroundColor = item.Key;
                    Console.WriteLine(item.Value);
                    Console.ForegroundColor = DEFAULT_FOREGROUND_COLOR;
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }

        public static void PublishException(Exception ex, string message = null)
        {
            var color = ConsoleColor.Yellow;
            message = string.IsNullOrEmpty(message) ? null : message + " ";
            string exMessage = string.Format("{0}\n{1}\n{2}", ex.Message, ex.Source, ex.StackTrace);
            if (!(ex is SocketException))
            {
                color = ConsoleColor.Red;
            }
            Debug(string.Format("{0}{1}\n{2}\n", message, ex.GetType().ToString(), exMessage), color);

            // Publish inner exception
            if (ex.InnerException != null)
            {
                PublishException(ex.InnerException, "Inner Exception:");
            }
        }
    }
}
