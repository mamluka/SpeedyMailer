using SpeedyMailer.Domain.Model.Lists;

namespace SpeedyMailer.Domain.DataAccess.Lists
{
    public interface IListRepository
    {
        ListsStore Lists();
        void Add(ListDescriptor listDescriptor);
        void Remove(string id);
        void Update(ListDescriptor listDescriptor);

    }
}