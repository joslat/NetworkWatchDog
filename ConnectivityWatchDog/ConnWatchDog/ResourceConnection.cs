using ConnWatchDog.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace ConnWatchDog
{
    public class ResourceConnection : IResourcePublisher
    {
        public string IP { get; private set; }
        public ConnectivityState ConnectionState { get; private set; }
        public IPStatus LastStatus { get; private set; } // Technically it can be obtained from the PingReply..
        public PingReply LastPingReply { get; private set; }
        public TimeSpan LastConnectionTime { get; private set; }
        public long TotalPings { get; set; }
        public long TotalSuccessfulPings { get; set; }
        private Stopwatch stopWatch = new Stopwatch();
        public bool StateChanged { get; private set; }

        // Member for Subscriber management
        List<IResourceSubscriber> ListOfSubscribers;

        public ResourceConnection(string ip)
        {
            ConnectionState = ConnectivityState.Disconnected; // first we asume its disconnection until we prove opposite.
            LastStatus = IPStatus.Unknown;
            TotalPings = 0;
            TotalSuccessfulPings = 0;
            stopWatch.Start();
            IP = ip;
            StateChanged = false;

            ListOfSubscribers = new List<IResourceSubscriber>();
        }

        public void AddPingResult(PingReply pr)
        {
            StateChanged = false;
            TotalPings++;
            LastPingReply = pr;
            LastStatus = pr.Status;

            if (pr.Status == IPStatus.Success)
            {
                stopWatch.Stop();
                LastConnectionTime = stopWatch.Elapsed;
                TotalSuccessfulPings++;
                stopWatch.Restart();

                if (ConnectionState == ConnectivityState.Disconnected)
                    StateChanged = true;
                ConnectionState = ConnectivityState.Connected;
            }
            else // no success..
            {
                if (ConnectionState == ConnectivityState.Connected)
                    StateChanged = true;
                ConnectionState = ConnectivityState.Disconnected; 
            }

            // We trigger the observer event so everybody subscribed gets notified
            if (StateChanged)
            {
                NotifySubscribers();
            }
        }

        /// <summary>
        ///  Interface implemenation for Observer pattern (IPublisher)
        /// </summary>
        public void RegisterSubscriber(IResourceSubscriber subscriber)
        {
            ListOfSubscribers.Add(subscriber);
        }
        public void RemoveSubscriber(IResourceSubscriber subscriber)
        {
            ListOfSubscribers.Remove(subscriber);
        }
        public void NotifySubscribers()
        {
            Parallel.ForEach(ListOfSubscribers, subscriber => {
                subscriber.Update(this);
            });
        }
    }
}
