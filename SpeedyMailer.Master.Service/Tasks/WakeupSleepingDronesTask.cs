using Quartz;
using Raven.Client;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Tasks;
using System.Linq;

namespace SpeedyMailer.Master.Service.Tasks
{
	public class WakeupSleepingDronesTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInSeconds(5).RepeatForever());
		}

		public class Job : IJob
		{
			private readonly IDocumentStore _documentStore;
			private Api _api;

			public Job(IDocumentStore documentStore,Api api)
			{
				_api = api;
				_documentStore = documentStore;
			}

			public void Execute(IJobExecutionContext context)
			{
				using (var session = _documentStore.OpenSession())
				{
					if (session.Query<CreativeFragment>().Customize(x=> x.WaitForNonStaleResults()).Any())
					{
						var drones = session.Query<Drone>().Customize(x => x.WaitForNonStaleResults()).Where(x => x.Status == DroneStatus.Asleep).ToList();
	
					}
					
				}
			}
		}
	}
}