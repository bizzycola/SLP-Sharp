using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using CommandLine;
using SwitchLanNet.Helpers;
using System.Net;

namespace SwitchLanNet
{
    public class Program
    {
        internal static SLPServer _slpServer;

        public static void Main(string[] args)
        {
            Console.WriteLine("Switch Lan Play");
            Console.WriteLine("===============");
            Console.WriteLine("C# Version by Bizzycola, based on SpaceMeowx2's NodeJS Server\n\n");

            Parser.Default.ParseArguments<CmdOptions>(args)
                .WithParsed(o =>
                {
                    RunArgs(o, args);
                })
                .WithNotParsed(o =>
                {
                    Console.WriteLine("Failed to parse command line arguments.");
                });
            
        }

        static void RunArgs(CmdOptions opt, string[] args)
        {
            int port = opt.Port;
            var cts = new CancellationTokenSource();


            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("[INFO] Exiting server..");
                cts.Cancel();
            };

            // parse IP option
            if(!IPAddress.TryParse(opt.Address, out var ip))
            {
                Console.WriteLine("[ERROR] Invalid IP address provided.");
                return;
            }

            Console.WriteLine($"SLP Listening on {ip.ToString()}:{port}...");

            _slpServer = new SLPServer(port, cts);
            _slpServer.Run();

            CreateWebHostBuilder(ip, port, args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(IPAddress ip, int port, string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseUrls($"http://{ip.ToString()}:{port}")
                .UseStartup<Startup>();
    }
}
