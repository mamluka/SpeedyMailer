using System.Collections.Generic;
using System.Linq;
using Quartz;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

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
			private readonly ParsePostfixLogsCommand _parsePostfixLogsCommand;
			private readonly LogsStore _logsStore;

			public Job(EventDispatcher eventDispatcher, ParsePostfixLogsCommand parsePostfixLogsCommand, LogsStore logsStore)
			{
				_logsStore = logsStore;
				_parsePostfixLogsCommand = parsePostfixLogsCommand;
				_eventDispatcher = eventDispatcher;
			}

			public void Execute(IJobExecutionContext context)
			{
				_parsePostfixLogsCommand.Logs = _logsStore.GetAllLogs();
				var parsedLogs = _parsePostfixLogsCommand.Execute();

				DispatchEvent<AggregatedMailBounced>(parsedLogs, MailEventType.Bounced);
				DispatchEvent<AggregatedMailSent>(parsedLogs, MailEventType.Sent);
				DispatchEvent<AggregatedMailDeferred>(parsedLogs, MailEventType.Deferred);
			}

			private void DispatchEvent<T>(IEnumerable<MailEvent> parsedLogs, MailEventType mailEventType) where T : AggregatedMail, new()
			{
				var mailEvent = new T
									{
										MailEvents = parsedLogs
											.Where(x => x.Type == mailEventType)
											.ToList()
									};

				_eventDispatcher.ExecuteAll(mailEvent);
			}
		}
	}
}