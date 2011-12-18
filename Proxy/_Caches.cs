using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace Loye.Proxy
{
    public static class DnsCache
    {
        private static readonly ConcurrentDictionary<string, DnsValue> _dnsDictionary = new ConcurrentDictionary<string, DnsValue>();

        public static int ExpireMinutes = DnsConfig.EXPIRE_MINUTES;

        public static IPAddress GetIPAddress(string hostNameOrAddress)
        {
            if (string.IsNullOrEmpty(hostNameOrAddress))
            {
                return null;
            }

            DnsValue value;
            if (_dnsDictionary.TryGetValue(hostNameOrAddress, out value)
                && (value.ExpireTime > DateTime.Now || value.ExpireTime == DateTime.MinValue))
            {
                return value.Address;
            }

            IPAddress address = null;
            if (IPAddress.TryParse(hostNameOrAddress, out address))
            {
                return address;
            }

            try
            {
                address = Dns.GetHostEntry(hostNameOrAddress).AddressList
                    .First(a => a.AddressFamily == AddressFamily.InterNetwork); //ipv4 only
            }
            catch (SocketException)
            {
                return null;
            }
            if (address != null)
            {
                _dnsDictionary.AddOrUpdate(
                    hostNameOrAddress,
                    new DnsValue(address, DateTime.Now.AddMinutes(ExpireMinutes)),
                    (k, v) => new DnsValue(address, DateTime.Now.AddMinutes(ExpireMinutes)));
            }
            return address;
        }

        public static void AppendDnsList(List<KeyValuePair<string, IPAddress>> dnsList, bool neverExpire = false)
        {
            DateTime expireTime = neverExpire ? DateTime.MinValue : DateTime.Now.AddMinutes(ExpireMinutes);
            if (dnsList != null)
            {
                dnsList.ForEach(i =>
                    {
                        _dnsDictionary.TryAdd(i.Key, new DnsValue(i.Value, expireTime));
                    });
            }
        }

        public static void Clear()
        {
            _dnsDictionary.Clear();
        }

        public static List<KeyValuePair<string, IPAddress>> GetDnsList()
        {
            return _dnsDictionary
                .Select(i => new KeyValuePair<string, IPAddress>(i.Key, i.Value.Address))
                .ToList();
        }

        public static string ToDebugString()
        {
            StringBuilder sb = new StringBuilder();
            var dnsList = _dnsDictionary.ToList();
            sb.Append(string.Format("Dns Count: {0};\tExpire Time: {1}min\n", dnsList.Count(), ExpireMinutes));
            dnsList.ForEach(i =>
                {
                    sb.Append(string.Format("[{0}] {1,-15} {2}\n", i.Value.ExpireTime.ToString("HH:mm:ss"), i.Value.Address, i.Key));
                });
            return sb.ToString();
        }


        #region Inner Structure

        private struct DnsValue
        {
            public DnsValue(IPAddress address, DateTime expireTime)
            {
                Address = address;
                ExpireTime = expireTime;
            }
            public IPAddress Address;
            public DateTime ExpireTime;
        }

        #endregion
    }

}
