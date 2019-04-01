namespace ConnWatchDog.Interfaces
{
    public interface IWatchdogPublisher
    {
        void RegisterSubscriber(IWatchdogSubscriber subscriber);
        void RemoveSubscriber(IWatchdogSubscriber subscriber);
        void NotifySubscribers();
    }
}
