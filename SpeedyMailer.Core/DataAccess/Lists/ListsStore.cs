using System.Collections.Generic;
using SpeedyMailer.Domain.Lists;

namespace SpeedyMailer.Core.DataAccess.Lists
{
    public class ListsStore
    {
        public ListsStore()
        {
            Lists = new List<ListDescriptor>();
        }

        public List<ListDescriptor> Lists { get; set; }
    }
}