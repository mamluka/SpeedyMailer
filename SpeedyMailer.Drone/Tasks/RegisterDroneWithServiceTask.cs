using System;
using Quartz;
using RestSharp;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drone.Tasks
{
	public class RegisterDroneWithServiceTask : ScheduledTaskWithData<RegisterDroneWithServiceTask.Data>
	{
		public RegisterDroneWithServiceTask(Action<Data> action)
			: base(action)
		{ }

		public override IJobDetail GetJob()
		{
			return SimpleJob<Job>(TaskData);
		}

		public override ITrigger GetTrigger()
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