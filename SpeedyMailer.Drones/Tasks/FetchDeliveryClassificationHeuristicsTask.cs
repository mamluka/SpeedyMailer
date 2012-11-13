using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class FetchDeliveryClassificationHeuristicsTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInHours(2).RepeatForever());
		}

		public class Job : IJob
		{
			private readonly OmniRecordManager _omniRecordManager;
			private readonly Api _api;

			public Job(Api api, OmniRecordManager omniRecordManager)
			{
				_api = api;
				_omniRecordManager = omniRecordManager;
			}

			public void Execute(IJobExecutionContext context)
			{
				var result = _api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, UnDeliveredMailClassificationHeuristicsRules>();

				if (result == null)
					return;

				_omniRecordManager.BatchInsert(new[] { result });
			}
		}
	}
}