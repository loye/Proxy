using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Net.Sockets;

namespace Loye.Proxy
{
    public static class DebugHelper
    {
        private static ConcurrentQueue<MessageItem> _printerQueue = new ConcurrentQueue<MessageItem>();

        private static readonly ConsoleColor DEFAULT_FOREGROUND_COLOR = Console.ForegroundColor;

        public static void Debug(string message, ConsoleColor? color = null)
        {
            if (message != null)
            {
                _printerQueue.Enqueue(new MessageItem() { Color = color ?? DEFAULT_FOREGROUND_COLOR, Message = message });
            }
        }

        public static void Debug(object message, ConsoleColor? color = null)
        {
            if (message != null)
            {
                DebugHelper.Debug(message.ToString(), color);
            }
        }

        public static void Print()
        {
            MessageItem item;
            while (true)
            {
                if (_printerQueue.TryDequeue(out item))
                {
                    Console.ForegroundColor = item.Color;
                    Console.WriteLine(item.Message);
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
            var color = ConsoleColor.Red;
            message = string.IsNullOrEmpty(message) ? null : message + " ";
            string exMessage = string.Format("{0}\n{1}\n{2}", ex.Message, ex.Source, ex.StackTrace);
            if (ex is SocketException)
            {
                color = ConsoleColor.Yellow;
            }
            Debug(string.Format("{0}{1}\n{2}\n", message, ex.GetType().ToString(), exMessage), color);

            // Publish inner exception
            if (ex.InnerException != null)
            {
                PublishException(ex.InnerException, "Inner Exception:");
            }
        }
    }

    internal struct MessageItem
    {
        public ConsoleColor Color;

        public string Message;
    }
}
