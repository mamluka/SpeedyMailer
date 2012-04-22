using System.Linq;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Core.DataAccess.Fragments
{
    public class FragmentRepository : IFragmentRepository
    {
        private readonly IDocumentStore store;

        public FragmentRepository(IDocumentStore store)
        {
            this.store = store;
        }


        public void Add(EmailFragment fragment)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Store(fragment);
            }
        }

        public EmailFragment PopFragment()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;

                EmailFragment emailFragment = session.Query<EmailFragment>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.Locked == false)
                    .OrderByDescending(x => x.CreateDate).Take(1).FirstOrDefault();


                if (emailFragment != null)
                {
                    try
                    {
                        emailFragment.Locked = true;

                        session.SaveChanges();
                        return emailFragment;
                    }
                    catch (ConcurrencyException)
                    {
                        return PopFragment();
                    }
                }
                return null;
            }
        }

    }
}