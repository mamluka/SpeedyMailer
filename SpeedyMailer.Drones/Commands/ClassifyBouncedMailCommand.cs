using System.Linq;
using System.Text.RegularExpressions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Commands
{
	public class ClassifyNonDeliveredMailCommand : Command<BounceType>
	{
		private readonly OmniRecordManager _omniRecordManager;

		public string Message { get; set; }

		public ClassifyNonDeliveredMailCommand(OmniRecordManager omniRecordManager)
		{
			_omniRecordManager = omniRecordManager;
		}

		public override BounceType Execute()
		{
			var heuristics = _omniRecordManager.GetSingle<UnDeliveredMailClassificationHeuristicsRules>();

			if (heuristics == null)
				return BounceType.NotClassified;

			var rules = heuristics
				.HardBounceRules
				.EmptyIfNull()
				.Select(x => new {Rule = x, BounceType = BounceType.HardBounce})
				.Union(heuristics
					       .IpBlockingRules
					       .EmptyIfNull()
					       .Select(x => new {Rule = x, BounceType = BounceType.IpBlocked})
				);

			var hardBounce = rules.FirstOrDefault(x => Regex.Match(Message, x.Rule.Condition).Success);

			return hardBounce != null ? hardBounce.BounceType : BounceType.NotClassified;
		}
	}
}