using Raven.Client;
using SpeedyMailer.Domain.Lists;

namespace SpeedyMailer.Core.DataAccess.Lists
{
    public class ListRepository : IListRepository
    {
        private readonly IDocumentStore store;

        public ListRepository(IDocumentStore store)
        {
            this.store = store;
        }


        public ListsStore Lists()
        {
            using (var session = store.OpenSession())
            {
                var list = session.Load<ListsStore>("system/lists");
                if (list == null)
                {
                    return new ListsStore();
                }
                return list;
            }
        }

        public void Add(ListDescriptor listDescriptor)
        {
            var listCollection = Lists();
            listCollection.Lists.Add(listDescriptor);
            using (var session = store.OpenSession())
            {
                session.Store(listCollection, "system/lists");
                session.SaveChanges();
            }
        }

        public void Remove(string id)
        {
            var listCollection = Lists();
            listCollection.Lists.RemoveAll(x => x.Id == id);
            using (var session = store.OpenSession())
            {
                session.Store(listCollection, "system/lists");
                session.SaveChanges();
            }
        }

        public void Update(ListDescriptor listDescriptor)
        {
            var listCollection = Lists();
            listCollection.Lists.RemoveAll(x => x.Id == listDescriptor.Id);
            listCollection.Lists.Add(listDescriptor);
            using (var session = store.OpenSession())
            {
                session.Store(listCollection, "system/lists");
                session.SaveChanges();
            }
        }

    }
}