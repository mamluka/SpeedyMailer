using System;
using Raven.Client;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Protocol;

namespace SpeedyMailer.EmailPool.Core.Emails
{
    public class MailOporations:IMailOporations
    {
        private readonly IDocumentStore store;

        public MailOporations(IDocumentStore store)
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

        public void Preform(DroneSideOporationBase poolSideOporation)
        {
            throw new NotImplementedException();
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