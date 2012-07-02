using System;
using Quartz;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drones.Tasks
{
	public class BroadcastDroneToServiceTask : ScheduledTaskWithData<BroadcastDroneToServiceTask.Data>
	{
		public BroadcastDroneToServiceTask(Action<Data> action)
			: base(action)
		{ }

		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x =>
											x.WithIntervalInMinutes(1)
											.RepeatForever()
				);
		}

		public class Data : ScheduledTaskData
		{
			public string Identifier { get; set; }
		}

		public class Job : JobBase<Data>, IJob
		{
			private readonly Api _api;

			public Job(Api api)
			{
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				var data = GetData(context);
				_api.Call<ServiceApi.RegisterDrone>()
					.WithParameters(x => x.Identifier = data.Identifier)
					.Get();
			}
		}
	}
}