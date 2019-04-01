using ConnWatchDog.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace ConnWatchDog
{
    public class NetworkWatchdogService : IWatchdogPublisher, INetworkWatchdog
    {
        List<ResourceConnection> ListOfConnectionsWatched;
        List<IWatchdogSubscriber> ListOfSubscribers;
        int RefreshTime = 0;
        int PingTimeout = 0;
        public CancellationToken cancellationToken { get; set; }
        bool NotifyOnlyWhenChanges = false;
        bool IsConfigured = false;

        public NetworkWatchdogService()
        {
            ListOfConnectionsWatched = new List<ResourceConnection>();
            ListOfSubscribers = new List<IWatchdogSubscriber>();
        }

        /// <summary>
        ///  Interface implemenation for Observer pattern (IPublisher)
        /// </summary>
        public void RegisterSubscriber(IWatchdogSubscriber subscriber)
        {
            ListOfSubscribers.Add(subscriber);
        }
        public void RemoveSubscriber(IWatchdogSubscriber subscriber)
        {
            ListOfSubscribers.Remove(subscriber);
        }
        public void NotifySubscribers()
        {
            Parallel.ForEach(ListOfSubscribers, subscriber => {
                subscriber.Update(ListOfConnectionsWatched);
            });
        }

        /// <summary>
        ///  Interfaces for the Network Watchdog
        /// </summary>
        
        public void AddResourceToWatch(string IP)
        {
            ResourceConnection rc = new ResourceConnection(IP);
            ListOfConnectionsWatched.Add(rc);
        }

        public void ConfigureWatchdog(int RefreshTime = 30000, int PingTimeout = 500, bool notifyOnlyWhenChanges = true)
        {
            this.RefreshTime = RefreshTime;
            this.PingTimeout = PingTimeout;
            this.NotifyOnlyWhenChanges = notifyOnlyWhenChanges;
            cancellationToken = new CancellationToken();
            IsConfigured = true;
        }

        public void Start()
        {
            StartWatchdogService();
        }

        public void Stop()
        {
            cancellationToken = new CancellationToken(true);
        }

        private async void StartWatchdogService()
        {
            var tasks = new List<Task>();

            if (IsConfigured) {
                while (!cancellationToken.IsCancellationRequested)
                {
                    foreach (var resConn in ListOfConnectionsWatched)
                    {
                        Ping p = new Ping();
                        var t = PingAndUpdateAsync(p, resConn.IP, PingTimeout);
                        tasks.Add(t);
                    }

                    if (this.NotifyOnlyWhenChanges)
                    {
                        await Task.WhenAll(tasks).ContinueWith(t =>
                        {
                        // now we can send the notification ... if any resources has changed its state from connected <==> disconnected 
                        if (ListOfConnectionsWatched.Any(res => res.StateChanged == true))
                            {
                                NotifySubscribers();
                            }
                        });
                    }
                    else NotifySubscribers();

                    // After all resources are monitored, we delay until the next planned execution.
                    await Task.Delay(RefreshTime).ConfigureAwait(false);
                }
            }
            else
            {
                throw new Exception("Cannot start Watchdog not configured");
            }
        }

        private async Task PingAndUpdateAsync(Ping ping, string ip, int timeout)
        {
            var reply = await ping.SendPingAsync(ip, timeout);
            var res = ListOfConnectionsWatched.First(item => item.IP == ip);
            res.AddPingResult(reply);
        }
    }
}
