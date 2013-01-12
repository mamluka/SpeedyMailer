using System.Collections.Generic;
using System.IO;
using ARSoft.Tools.Net.Dns;
using Newtonsoft.Json;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Drones.Tasks
{
	public class DnsBlackListingIpNotificationTask:ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(5).RepeatForever());
		}

		public class Job:IJob
		{
			private DroneSettings _droneSettings;

			public Job(DroneSettings droneSettings)
			{
				_droneSettings = droneSettings;
			}

			public void Execute(IJobExecutionContext context)
			{
				var dnsbls = JsonConvert.DeserializeObject<List<Dnsbl>>(File.ReadAllText("data/dnsbl.js"));

				foreach (var dnsbl in dnsbls)
				{
					DnsClient.Default.Resolve(dnsbl.Dns, RecordType.A);
				}
				
			}
		}
	}
}