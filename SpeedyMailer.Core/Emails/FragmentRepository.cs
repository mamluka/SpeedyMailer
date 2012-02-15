using System.Linq;
using Raven.Abstractions.Exceptions;
using Raven.Client;

namespace SpeedyMailer.Core.Emails
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
            using (var session = store.OpenSession())
            {
                session.Store(fragment);
            }
        }

        public EmailFragment PopFragment()
        {
            using (var session = store.OpenSession())
            {
                session.Advanced.UseOptimisticConcurrency = true;

                var emailFragment = session.Query<EmailFragment>()
                    .Customize(x => x.WaitForNonStaleResults())
                    .Where(x => x.Locked == false)
                    .OrderByDescending(x => x.CreateDate)
                    .Take(1)
                    .SingleOrDefault();

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