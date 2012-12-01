using System.Linq;
using Quartz;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class SendDroneStateSnapshotTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(30).RepeatForever());
		}

		public class Job : IJob
		{
			private readonly Api _api;
			private readonly DroneSettings _droneSettings;
			private readonly LogsStore _logsStore;
			private readonly OmniRecordManager _omniRecordManager;

			public Job(Api api, LogsStore logsStore, DroneSettings droneSettings, OmniRecordManager omniRecordManager)
			{
				_omniRecordManager = omniRecordManager;
				_logsStore = logsStore;
				_droneSettings = droneSettings;
				_api = api;
			}

			public void Execute(IJobExecutionContext context)
			{
				var reducedLogs = _logsStore
					.GetAllLogs()
					.Select(x => new ReducedMailLogEntry
									 {
										 Level = x.level,
										 Message = x.msg,
										 Time = x.time
									 })
					.ToList();

				_api.Call<ServiceEndpoints.Drones.SendStateSnapshot>(x =>
																		 {
																			 x.Drone = new Drone
																						   {
																							   Id = _droneSettings.Identifier,
																							   BaseUrl = _droneSettings.BaseUrl,
																							   Domain = _droneSettings.Domain
																						   };

																			 x.RawLogs = reducedLogs;
																			 x.MailSent = _omniRecordManager.GetAll<MailSent>();
																			 x.MailBounced = _omniRecordManager.GetAll<MailBounced>();
																			 x.MailDeferred = _omniRecordManager.GetAll<MailDeferred>();
																			 x.UnsubscribeRequests = _omniRecordManager.GetAll<UnsubscribeRequest>();
																			 x.ClickActions = _omniRecordManager.GetAll<ClickAction>();
																		 });

				if (_api.ResponseStatus.DidAnErrorOccured())
					return;

				_omniRecordManager.DeleteConnection<MailLogEntry>();
				_omniRecordManager.DeleteConnection<MailSent>();
				_omniRecordManager.DeleteConnection<MailDeferred>();
				_omniRecordManager.DeleteConnection<MailBounced>();
				_omniRecordManager.DeleteConnection<ClickAction>();
				_omniRecordManager.DeleteConnection<UnsubscribeRequest>();
			}
		}
	}
}