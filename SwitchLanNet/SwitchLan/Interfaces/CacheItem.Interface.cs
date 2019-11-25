using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SwitchLanNet.Interfaces
{
    internal class CacheItem
    {
        public DateTime ExpireAt { get; set; }
        public IPEndPoint RInfo { get; set; }
    }
}
