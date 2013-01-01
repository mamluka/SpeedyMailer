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
			var heuristics = _omniRecordManager.GetSingle<DeliverabilityClassificationRules>();

			if (heuristics == null)
				return new MailClassfication { Classification = Classification.NotClassified };

			var rules = heuristics
				.HardBounceRules
				.EmptyIfNull()
				.Select(x => new { Condition = x, Classification = new MailClassfication { Classification = Classification.HardBounce, TimeSpan = TimeSpan.FromHours(0) } })
				.Union(heuristics
						   .BlockingRules
						   .EmptyIfNull()
						   .Select(x => new { x.Condition, Classification = new MailClassfication { Classification = Classification.Blocked, TimeSpan = x.TimeSpan } })
				);

			var hardBounce = rules.FirstOrDefault(x => Regex.Match(Message, x.Condition, RegexOptions.IgnoreCase).Success);

			return hardBounce != null ? hardBounce.Classification : new MailClassfication { Classification = Classification.NotClassified };
		}
	}
}