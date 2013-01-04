using System;
using System.Linq;
using System.Text.RegularExpressions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Commands
{
	public class ClassifyNonDeliveredMailCommand : Command<MailClassfication>
	{
		private readonly OmniRecordManager _omniRecordManager;

		public string Message { get; set; }

		public ClassifyNonDeliveredMailCommand(OmniRecordManager omniRecordManager)
		{
			_omniRecordManager = omniRecordManager;
		}

		public override MailClassfication Execute()
		{
			var heuristics = _omniRecordManager.Load<DeliverabilityClassificationRules>();

			if (heuristics == null)
				return new MailClassfication { Type = Classification.NotClassified };

			var rules = heuristics
				.Rules
				.EmptyIfNull();

			var matchedRule = rules.FirstOrDefault(x => Regex.Match(Message, x.Condition, RegexOptions.IgnoreCase).Success);

			return matchedRule != null ? new MailClassfication { Type = matchedRule.Type, TimeSpan = matchedRule.Data != null ? matchedRule.Data.TimeSpan : default(TimeSpan) } : new MailClassfication { Type = Classification.NotClassified };
		}
	}
}