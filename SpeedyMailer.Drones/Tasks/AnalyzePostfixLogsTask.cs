using System;
using System.Collections.Generic;
using System.Linq;
using Quartz;
using Raven.Client.Util;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Evens;
using SpeedyMailer.Core.Settings;
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
			return TriggerWithTimeCondition(x => x.WithIntervalInMinutes(10).RepeatForever());
		}

		[DisallowConcurrentExecution]
		public class Job : IJob
		{
			private readonly EventDispatcher _eventDispatcher;
			private readonly ParsePostfixLogsCommand _parsePostfixLogsCommand;
			private readonly LogsStore _logsStore;
			private readonly OmniRecordManager _omniRecordManager;
			private readonly IntervalRulesStore _intervalRulesStore;
			private readonly CreativeFragmentSettings _creativeFragmentSettings;

			public Job(EventDispatcher eventDispatcher,
				ParsePostfixLogsCommand parsePostfixLogsCommand,
				LogsStore logsStore,
				OmniRecordManager omniRecordManager,
				IntervalRulesStore intervalRulesStore,
				CreativeFragmentSettings creativeFragmentSettings)
			{
				_creativeFragmentSettings = creativeFragmentSettings;
				_intervalRulesStore = intervalRulesStore;
				_omniRecordManager = omniRecordManager;
				_logsStore = logsStore;
				_parsePostfixLogsCommand = parsePostfixLogsCommand;
				_eventDispatcher = eventDispatcher;
			}

			public void Execute(IJobExecutionContext context)
			{
				var mailLogEntries = _logsStore.GetUnprocessedLogs();

				_parsePostfixLogsCommand.Logs = mailLogEntries;
				var parsedLogs = _parsePostfixLogsCommand.Execute();

				var parsedLogsDomainGroups = CalculateDomainGroupFor(parsedLogs);

				var mailSent = ParseToSpecificMailEvent(parsedLogs, MailEventType.Sent, ToMailSent, parsedLogsDomainGroups);
				var mailBounced = ParseToSpecificMailEvent(parsedLogs, MailEventType.Bounced, ToMailBounced, parsedLogsDomainGroups);
				var mailDeferred = ParseToSpecificMailEvent(parsedLogs, MailEventType.Deferred, ToMailDeferred, parsedLogsDomainGroups);

				_omniRecordManager.BatchInsert(mailSent);
				_omniRecordManager.BatchInsert(mailBounced);
				_omniRecordManager.BatchInsert(mailDeferred);

				_logsStore.MarkProcessed(mailLogEntries);

				DispatchEvent<AggregatedMailBounced, MailBounced>(mailBounced);
				DispatchEvent<AggregatedMailSent, MailSent>(mailSent);
				DispatchEvent<AggregatedMailDeferred, MailDeferred>(mailDeferred);
			}

			private IDictionary<string, string> CalculateDomainGroupFor(IEnumerable<MailEvent> mailEvents)
			{
				var conditions = _intervalRulesStore
					.GetAll()
					.SelectMany(intervalRule => intervalRule.Conditons.Select(x => new { Condition = x, intervalRule.Group }))
					.ToList();

				return mailEvents
					.Distinct(new LambdaComparer<MailEvent>((x, y) => x.Recipient == y.Recipient))
					.Select(x => new { Group = conditions.Where(m => x.Recipient.Contains(m.Condition)).Select(m => m.Group).SingleOrDefault(), x.Recipient })
					.ToDictionary(x => x.Recipient, x => x.Group);
			}

			private List<TEventData> ParseToSpecificMailEvent<TEventData>(IEnumerable<MailEvent> parsedLogs, MailEventType mailEventType, Func<MailEvent, TEventData> convertFunction, IDictionary<string, string> parsedLogsDomainGroups) where TEventData : IHasDomainGroup, IHasRecipient
			{
				return parsedLogs
					.Where(x => x.Type == mailEventType)
					.Select(convertFunction)
					.Join(parsedLogsDomainGroups, x => x.Recipient, y => y.Key, SetDomainGroup)
					.ToList();
			}

			private TEventData SetDomainGroup<TEventData>(TEventData x, KeyValuePair<string, string> y) where TEventData : IHasDomainGroup, IHasRecipient
			{
				x.DomainGroup = y.Value ?? _creativeFragmentSettings.DefaultGroup;
				return x;
			}

			private static MailDeferred ToMailDeferred(MailEvent x)
			{
				return new MailDeferred
						   {
							   Recipient = x.Recipient,
							   Time = x.Time,
							   Message = x.RelayMessage
						   };
			}

			private static MailBounced ToMailBounced(MailEvent x)
			{
				return new MailBounced
						   {
							   Recipient = x.Recipient,
							   Time = x.Time,
							   Message = x.RelayMessage
						   };
			}

			private static MailSent ToMailSent(MailEvent x)
			{
				return new MailSent
						   {
							   Recipient = x.Recipient,
							   Time = x.Time
						   };
			}

			private void DispatchEvent<TEvent, TEventData>(IList<TEventData> mailEvents) where TEvent : AggregatedMailEvents<TEventData>, new()
			{
				if (!mailEvents.Any())
					return;

				var mailEvent = new TEvent
									{
										MailEvents = mailEvents
									};

				_eventDispatcher.ExecuteAll(mailEvent);
			}
		}
	}

	public class LambdaComparer<T> : IEqualityComparer<T>
	{
		private readonly Func<T, T, bool> _lambdaComparer;
		private readonly Func<T, int> _lambdaHash;

		public LambdaComparer(Func<T, T, bool> lambdaComparer) :
			this(lambdaComparer, o => 0)
		{
		}

		public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
		{
			if (lambdaComparer == null)
				throw new ArgumentNullException("lambdaComparer");
			if (lambdaHash == null)
				throw new ArgumentNullException("lambdaHash");

			_lambdaComparer = lambdaComparer;
			_lambdaHash = lambdaHash;
		}

		public bool Equals(T x, T y)
		{
			return _lambdaComparer(x, y);
		}

		public int GetHashCode(T obj)
		{
			return _lambdaHash(obj);
		}
	}
}