using SwitchLanNet.Interfaces;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SwitchLanNet
{
    internal static class Utils
    {
        public static string AddressToString(IPEndPoint addr)
            => $"{addr.Address.ToString()}:{addr.Port}";

        public static Dictionary<T, CacheItem> ClearCacheItem<T>(Dictionary<T, CacheItem> map)
        {
            var date = DateTime.Now;
            var val = map.FirstOrDefault(p => p.Value?.ExpireAt < date);
            if (val.Key != null)
                map.Remove(val.Key);

            return map;
        }
    }
}
