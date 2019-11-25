using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using SwitchLanNet.Interfaces;
using System.Threading.Tasks;
using SwitchLanNet.Enums;
using System.Threading;

namespace SwitchLanNet
{
    /// <summary>
    /// Switch Lan Play UDP Relay Server
    /// </summary>
    public class SLPServer
    {
        /// <summary>
        /// Timeout in seconds before removing clients from cache
        /// </summary>
        const int TIMEOUT = 30;

        /// <summary>
        /// IPv4 Packet Source Offset
        /// </summary>
        const int IPV4_OFF_SRC = 12;

        /// <summary>
        /// IPv4 Packet Destination Offset
        /// </summary>
        const int IPV4_OFF_DST = 16;

        /// <summary>
        /// Test and statistic data for this server
        /// </summary>
        public TestData TestData { get; }

        /// <summary>
        /// Returns the number of currently connected clients
        /// </summary>
        public int ClientCount { get { return _clients.Count; } }

        UdpClient _server;
        Dictionary<string, CacheItem> _clients;
        Dictionary<int, CacheItem> _ipCache;
        CancellationTokenSource _cts;

        public SLPServer(int port, CancellationTokenSource cts)
        {
            _clients = new Dictionary<string, CacheItem>();
            _ipCache = new Dictionary<int, CacheItem>();
            _cts = cts;
            TestData = new TestData();

            _server = new UdpClient(port);

        }

        /// <summary>
        /// Starts listening and running the reset loop
        /// </summary>
        public async void Run()
        {
            ResetData();
            RunLoop();
        }

        /// <summary>
        /// UDP Listen Loop
        /// </summary>
        async void RunLoop()
        {
            Console.WriteLine("[INFO] Running listen loop..");

            while (!_cts.IsCancellationRequested)
            {
                try
                {
                    // Recieve the packet and validate it contains data
                    var data = await _server.ReceiveAsync();

                    if (data.Buffer.Length <= 0)
                        continue;

                    // Update the download amount
                    TestData.Download += data.Buffer.Length;

                    // Determine the packet type by the first byte
                    var packetType = (ForwarderType)data.Buffer[0];

                    // If the packet is not a ping, update(or create) the timeout info on the client
                    if (packetType != ForwarderType.Ping)
                    {
                        var cstr = Utils.AddressToString(data.RemoteEndPoint);
                        if (_clients.ContainsKey(cstr))
                            _clients[cstr].ExpireAt = DateTime.Now.AddSeconds(TIMEOUT);                       
                        else
                        {
                            _clients.Add(cstr, new CacheItem()
                            {
                                ExpireAt = DateTime.Now.AddSeconds(TIMEOUT),
                                RInfo = data.RemoteEndPoint
                            });
                        }
                    }

                    OnPacket(data.RemoteEndPoint, packetType, data.Buffer.Skip(1).ToArray(), data.Buffer);
                }
                catch(Exception ex) 
                {
                    Console.WriteLine("[ERROR] Error in UDP Packet Loop: {0}", ex);
                }
            }

            _server.Close();
        }

        /// <summary>
        /// Determine which method to pass the packet to based on its ForwaderType
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="type"></param>
        /// <param name="payload"></param>
        /// <param name="msg"></param>
        void OnPacket(IPEndPoint ip, ForwarderType type, byte[] payload, byte[] msg)
        {
            switch (type)
            {
                case ForwarderType.Keepalive:
                    break;
                case ForwarderType.Ipv4:
                    OnIp4v(ip, payload, msg);
                    break;
                case ForwarderType.Ping:
                    OnPing(ip, payload, msg);
                    break;
                case ForwarderType.Ipv4Frag:
                    OnIp4vFrag(ip, payload, msg);
                    break;

                default:
                    Console.WriteLine("Unknown packet type");
                    break;
            }
        }

        #region Packet Handler Methods
        void OnIp4vFrag(IPEndPoint fromAddr, byte[] payload, byte[] msg)
        {
            if (payload.Length <= 20) return;

            var src = BitConverter.ToInt32(payload, 0);
            var dst = BitConverter.ToInt32(payload, 4);

            _ipCache[src] = new CacheItem()
            {
                RInfo = fromAddr,
                ExpireAt = DateTime.Now.AddSeconds(TIMEOUT)
            };

            if (_ipCache.ContainsKey(dst))
                SendTo(_ipCache[dst].RInfo, msg);
            else
                SendBroadcast(fromAddr, msg);
        }

        void OnPing(IPEndPoint fromAddr, byte[] payload, byte[] msg) 
            => SendTo(fromAddr, msg);

        void OnIp4v(IPEndPoint fromAddr, byte[] payload, byte[] msg)
        {
            if (payload.Length <= 20) return;

            var src = BitConverter.ToInt32(payload, IPV4_OFF_SRC);
            var dst = BitConverter.ToInt32(payload, IPV4_OFF_DST);

            _ipCache[src] = new CacheItem()
            {
                RInfo = fromAddr,
                ExpireAt = DateTime.Now.AddSeconds(TIMEOUT)
            };

            if (_ipCache.ContainsKey(dst))
                SendTo(_ipCache[dst].RInfo, msg);
            else
                SendBroadcast(fromAddr, msg);
        }
        #endregion

        /// <summary>
        /// Send data to another connected client
        /// </summary>
        /// <param name="addr">Endpoint to send data to</param>
        /// <param name="data">Byte array of packet data to send</param>
        void SendTo(IPEndPoint addr, byte[] data)
        {
            try
            {
                TestData.Upload += data.Length;
                _server.Send(data, data.Length, addr);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (ex is SocketException)
                    _clients.Remove(Utils.AddressToString(addr));
            }
        }

        /// <summary>
        /// Send data to all connected clients except sender
        /// </summary>
        /// <param name="except">Sender endpoint to avoid sending to</param>
        /// <param name="data">Byte array of packet data to send</param>
        void SendBroadcast(IPEndPoint except, byte[] data)
        {
            var exceptStr = Utils.AddressToString(except);
            var clientsExcept = _clients.Where(p => p.Key != exceptStr);

            foreach (var client in clientsExcept)
                SendTo(client.Value.RInfo, data);

        }

        /// <summary>
        /// Runs a loop to reset upload/download speed stats per second and clear timed-out clients
        /// </summary>
        async void ResetData()
        {
            Console.WriteLine("[INFO] Running reset loop..");

            while(!_cts.IsCancellationRequested)
            {
                try
                {
                    TestData.Upload = 0;
                    TestData.Download = 0;

                    _clients = Utils.ClearCacheItem(_clients);
                    _ipCache = Utils.ClearCacheItem(_ipCache);

                    await Task.Delay(1000);
                }
                catch { }
            }
        }
    }

    /// <summary>
    /// Holds test data and statistics for use by the web API
    /// </summary>
    public class TestData
    {
        /// <summary>
        /// Upload speed in bytes/s
        /// </summary>
        public long Upload { get; set; }

        /// <summary>
        /// Download speed in bytes/s
        /// </summary>
        public long Download { get; set; }



        public TestData()
        {
            Upload = 0;
            Download = 0;
        }
    }
}
