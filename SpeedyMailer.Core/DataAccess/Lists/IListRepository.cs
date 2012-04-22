using SpeedyMailer.Core.Domain.Lists;

namespace SpeedyMailer.Core.DataAccess.Lists
{
    public interface IListRepository
    {
        ListsStore Lists();
        void Add(ListDescriptor listDescriptor);
        void Remove(string id);
        void Update(ListDescriptor listDescriptor);
    }
}