namespace ConnWatchDog.Interfaces
{
    public interface INetworkWatchdog
    {
        void AddResourceToWatch(string IP);
        void ConfigureWatchdog(int RefreshTime, int PingTimeout, bool notifyOnlyWhenChanges);
        void Start();
        void Stop();
    }
}
