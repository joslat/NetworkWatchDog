using System.Collections.Generic;

namespace ConnWatchDog.Interfaces
{
    public interface IWatchdogSubscriber
    {
        void Update(List<ResourceConnection> data);
    }
}
