using System.Collections.Generic;

namespace SpeedyMailer.Core.Lists
{
    public interface IListRepository
    {
        ListCollection Lists();
        void Add(ListDescriptor listDescriptor);
        void Remove(string id);
        void Update(ListDescriptor listDescriptor);

    }
}