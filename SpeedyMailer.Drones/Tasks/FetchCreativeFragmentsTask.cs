using System;
using System.Diagnostics;
using Quartz;
using RestSharp;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drones.Tasks
{
	public class FetchCreativeFragmentsTask:ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(1).WithRepeatCount(1));
		}

		public class Job:IJob
		{
			private readonly Api _api;

			public Job(Api api)
			{
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				_api.Call<ServiceEndpoints.FetchFragment>().Get();
			}
		}
	}
}