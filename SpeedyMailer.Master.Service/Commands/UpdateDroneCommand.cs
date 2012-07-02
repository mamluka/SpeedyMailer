using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Master.Service.Commands
{
	public class UpdateDroneCommand:Command
	{
		private readonly IDocumentStore _documentStore;

		public Drone Drone { get; set; }

		public UpdateDroneCommand(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute()
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(Drone);
				session.SaveChanges();
			}	
		}
	}
}