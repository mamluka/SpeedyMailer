using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using ARSoft.Tools.Net.Dns;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Tasks
{
	public class BlacklistingDnsCheckTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(5).RepeatForever());
		}

		public class Job : IJob
		{
			private readonly DroneSettings _droneSettings;
			private readonly OmniRecordManager _omniRecordManager;

			public Job(DroneSettings droneSettings, OmniRecordManager omniRecordManager)
			{
				_omniRecordManager = omniRecordManager;
				_droneSettings = droneSettings;
			}

			public void Execute(IJobExecutionContext context)
			{
				var dnsbls = JsonConvert.DeserializeObject<List<Dnsbl>>(File.ReadAllText("data/dnsbl.js"));


				var reverseIp = string.Join(".", _droneSettings.Ip.Split('.').Reverse());
				var results = dnsbls.AsParallel().SelectMany(x =>
					{
						var name = reverseIp + "." + x.Dns;
						Console.WriteLine(name);

						var client = new DnsClient(IPAddress.Parse("8.8.8.8"), 1);
						var records = client.Resolve(name, RecordType.A);
						return records.AnswerRecords.OfType<ARecord>().Select(p => new BlacklistingRecord { Result = p.Address.ToString(), Service = x.Dns });
					}).ToList();

				_omniRecordManager.UpdateOrInsert(new BlacklistingIpInformation
					{
						Answers = results
					});
			}
		}
	}

	public class BlacklistingRecord
	{
		public string Service { get; set; }
		public string Result { get; set; }
	}

	public class BlacklistingIpInformation
	{
		[BsonId(IdGenerator = typeof(TypeNameIdGenerator))]
		public string Id { get; set; }

		public IList<BlacklistingRecord> Answers { get; set; }
	}
}