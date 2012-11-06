using System.Collections.Generic;
using Mongol;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;

namespace SpeedyMailer.Drones.Tasks
{
	public class AnalyzePostfixLogsTask : ScheduledTask
	{
		public override IJobDetail ConfigureJob()
		{
			return SimpleJob<Job>();
		}

		public override ITrigger ConfigureTrigger()
		{
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(1).RepeatForever());
		}

		[DisallowConcurrentExecution]
		public class Job : IJob
		{
			private readonly EventDispatcher _eventDispatcher;
			private ParsePostfixLogsCommand _parsePostfixLogsCommand;

			public Job(EventDispatcher eventDispatcher, ParsePostfixLogsCommand parsePostfixLogsCommand, LogsStore logsStore)
			{
				_parsePostfixLogsCommand = parsePostfixLogsCommand;
				_eventDispatcher = eventDispatcher;
			}

			public void Execute(IJobExecutionContext context)
			{

				//_parsePostfixLogsCommand.Logs = 
				_eventDispatcher.ExecuteAll(new AggregatedMailSent());
			}
		}
	}

	public class LogsStore : RecordManager<MailLogEntry>
	{
		public LogsStore(DroneSettings droneSettings)
			: base(droneSettings.StoreHostname)
		{ }

		public IList<MailLogEntry> GetAllLogs()
		{
			return 
		}
	}
}