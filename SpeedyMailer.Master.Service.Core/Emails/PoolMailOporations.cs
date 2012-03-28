using System;
using Raven.Client;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Master.Service.Core.Emails
{
    public class PoolMailOporations:IPoolMailOporations
    {
        private readonly IDocumentStore store;

        public PoolMailOporations(IDocumentStore store)
        {
            this.store = store;
        }

        public void Preform(PoolSideOporationBase poolSideOporation)
        {

            switch (poolSideOporation.FragmentOporationType)
            {
                case PoolFragmentOporationType.SetAsCompleted:
                    SetAsComplete(poolSideOporation as FragmentCompleteOporation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            
        }

        private void SetAsComplete(FragmentCompleteOporation fragmentCompleteOporation)
        {
            using (var session = store.OpenSession())
            {
                var fragment = session.Load<EmailFragment>(fragmentCompleteOporation.FragmentId);
                fragment.Status = FragmentStatus.Completed;
                fragment.CompletedBy = fragmentCompleteOporation.CompletedBy;

                session.Store(fragment);
                session.SaveChanges();
            }
        }
    }
}