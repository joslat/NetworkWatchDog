namespace ConnWatchDog.Interfaces
{
    public interface IResourcePublisher
    {
        void RegisterSubscriber(IResourceSubscriber subscriber);
        void RemoveSubscriber(IResourceSubscriber subscriber);
        void NotifySubscribers();
    }
}
