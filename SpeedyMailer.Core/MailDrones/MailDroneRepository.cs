using System.Collections.Generic;
using Raven.Client;
using Raven.Client.Linq;
using System.Linq;
namespace SpeedyMailer.Core.MailDrones
{
    public class MailDroneRepository:IMailDroneRepository
    {
        private readonly IDocumentStore store;

        public MailDroneRepository(IDocumentStore store)
        {
            this.store = store;
        }

        public List<MailDrone> SleepingDrones()
        {
            using (var session = store.OpenSession())
            {
                var drones = session.Query<MailDrone>()
                    .Where(x => x.Status == DroneStatus.Asleep)
                    .Customize(x => x.WaitForNonStaleResults()).ToList();

                return drones;
            }
        }

        public void Update(MailDrone mailDrone)
        {
            using (var session = store.OpenSession())
            {
                session.Store(mailDrone);
                session.SaveChanges();
            }
        }
    }
}