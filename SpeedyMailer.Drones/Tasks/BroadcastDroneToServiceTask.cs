using System;
using System.Linq;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Logging;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

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
											x.WithIntervalInMinutes(5)
											.RepeatForever()
				);
		}

		public class Job : IJob
		{
			private readonly Api _api;
			private readonly DroneSettings _droneSettings;
			private readonly OmniRecordManager _omniRecordManager;

			public Job(Api api,DroneSettings droneSettings, OmniRecordManager omniRecordManager)
			{
				_omniRecordManager = omniRecordManager;
				_droneSettings = droneSettings;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				_api.Call<ServiceEndpoints.Drones.RegisterDrone>(x =>
					{
						x.Identifier = _droneSettings.Identifier;
						x.BaseUrl = _droneSettings.BaseUrl;
						x.LastUpdate = DateTime.UtcNow.ToLongTimeString();
						x.Domain = _droneSettings.Domain;
						x.IpReputation = _omniRecordManager.Load<IpReputation>();
						x.Exceptions =
							_omniRecordManager.GetAll<DroneExceptionLogEntry>()
							                  .Select(entry => new DroneException {Component = entry.component, Time = entry.time, Message = entry.message, Exception = entry.exception})
							                  .ToList();
					});
			}
		}
	}
}