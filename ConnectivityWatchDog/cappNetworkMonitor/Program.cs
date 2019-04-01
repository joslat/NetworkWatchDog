using ConnWatchDog;
using ConnWatchDog.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace cappNetworkMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("About to start the Network Watchdog, will finish when configured (1 minutes)...");
            var ap = new AsyncPinger();
            ap.RunPingSweep_Async();

            Console.ReadLine();
        }
    }

    public class AsyncPinger : IWatchdogSubscriber
    {
        private string BaseIP = "192.168.178.";
        private int StartIP = 1;
        private int StopIP = 255;
        private string ip;
        private int timeout = 1000;
        private int heartbeat = 10000;
        private NetworkWatchdogService nws;

        public AsyncPinger()
        {
            nws = new NetworkWatchdogService();
            nws.ConfigureWatchdog(heartbeat, timeout, false);
        }

        public async void RunPingSweep_Async()
        {           
            var tasks = new List<Task>();

            for (int i = StartIP; i < StopIP; i++)
            {
                ip = BaseIP + i.ToString();
                nws.AddResourceToWatch(ip);
            }

            nws.RegisterSubscriber(this);
            var cts = new CancellationTokenSource();
            cts.CancelAfter(60000);
            nws.cancellationToken = cts.Token;
            nws.Start();
        }

        public void Update(List<ResourceConnection> data)
        {
            Console.WriteLine("Update from the Network watcher!");
            foreach (var res in data)
            {
                if (res.ConnectionState == ConnectivityState.Connected)
                {
                    Console.WriteLine("Received from " + res.IP + " total pings: " + res.TotalPings.ToString() + " successful pings: " + res.TotalSuccessfulPings.ToString());
                }
            }
            Console.WriteLine("End of Update ");
        }
    }
}
