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
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(10).RepeatForever());
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
				var result = _api.Call<ServiceEndpoints.Heuristics.GetDeliveryRules, DeliverabilityClassificationRules>();

				if (result == null)
					return;

				var rules = _omniRecordManager.GetSingle<DeliverabilityClassificationRules>() ?? new DeliverabilityClassificationRules();
				rules.Rules = result.Rules;

				_omniRecordManager.UpdateOrInsert(result);
			}
		}
	}
}