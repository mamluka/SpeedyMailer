using System.Collections.Generic;
using System.Linq;
using Raven.Client;
using Raven.Client.Linq;
using SpeedyMailer.Bridge.Model.Drones;
namespace SpeedyMailer.Core.DataAccess.Drone
{
    public class MailDroneRepository : IMailDroneRepository
    {
        private readonly IDocumentStore store;

        public MailDroneRepository(IDocumentStore store)
        {
            this.store = store;
        }


        public List<MailDrone> CurrentlySleepingDrones()
        {
            using (IDocumentSession session = store.OpenSession())
            {
                List<MailDrone> drones = session.Query<MailDrone>()
                    .Where(x => x.Status == DroneStatus.Asleep)
                    .Customize(x => x.WaitForNonStaleResults()).ToList();

                return drones;
            }
        }

        public void Update(MailDrone mailDrone)
        {
            using (IDocumentSession session = store.OpenSession())
            {
                session.Store(mailDrone);
                session.SaveChanges();
            }
        }

    }
}