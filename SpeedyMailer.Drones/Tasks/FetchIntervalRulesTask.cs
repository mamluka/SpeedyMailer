using System.Collections.Generic;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class FetchIntervalRulesTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(60).RepeatForever());
		}

		public class Job : IJob
		{
			private readonly Api _api;
			private readonly IntervalRulesStore _intervalRulesStore;

			public Job(Api api, IntervalRulesStore intervalRulesStore)
			{
				_intervalRulesStore = intervalRulesStore;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				var intervalRules = _api.Call<ServiceEndpoints.Rules.GetIntervalRules, List<IntervalRule>>();

				if (intervalRules == null)
					return;

				_intervalRulesStore.RemoveAll();
				_intervalRulesStore.BatchInsert(intervalRules);
			}
		}

	}
}