using System;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Settings;

namespace SpeedyMailer.Drones.Tasks
{
	public class BroadcastDroneToServiceTask : ScheduledTask
	{
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

		public class Job : IJob
		{
			private readonly Api _api;
			private readonly DroneSettings _droneSettings;

			public Job(Api api,DroneSettings droneSettings)
			{
				_droneSettings = droneSettings;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				_api.Call<ServiceEndpoints.RegisterDrone>(x =>
					{
						x.Identifier = _droneSettings.Identifier;
						x.BaseUrl = _droneSettings.BaseUrl;
					});
			}
		}
	}
}