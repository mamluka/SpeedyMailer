using System;
using Raven.Client;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.EmailPoolMaster.MailDrones;

namespace SpeedyMailer.EmailPoolMaster.Emails
{
    public class EmailOporations:IEMailOporations
    {
        private readonly IDocumentStore store;

        public EmailOporations(IDocumentStore store)
        {
            this.store = store;
        }

        public void Preform(PoolSideOporationBase poolSideOporation)
        {

            switch (poolSideOporation.FragmentOpotationType)
            {
                case FragmentOpotationType.SetAsCompleted:
                    SetAsComplete(poolSideOporation as FragmentComplete);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            
        }

        private void SetAsComplete(FragmentComplete fragmentComplete)
        {
            using (var session = store.OpenSession())
            {
                var fragment = session.Load<EmailFragment>(fragmentComplete.FragmentId);
                fragment.Status = FragmentStatus.Completed;
                fragment.CompletedBy = fragmentComplete.CompletedBy;

                session.Store(fragment);
                session.SaveChanges();
            }
        }
    }
}