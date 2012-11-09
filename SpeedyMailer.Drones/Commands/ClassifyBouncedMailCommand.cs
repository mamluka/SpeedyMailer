using System.Linq;
using System.Text.RegularExpressions;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Commands
{
	public class ClassifyBouncedMailCommand:Command<BounceType>
	{
		private readonly OmniRecordManager _omniRecordManager;

		public string Message { get; set; }

		public ClassifyBouncedMailCommand(OmniRecordManager omniRecordManager)
		{
			_omniRecordManager = omniRecordManager;
		}

		public override BounceType Execute()
		{
			var heuristics = _omniRecordManager.GetSingle<HardBounceHeuristics>();

			var hardBounce = heuristics.HardBounceRules.Any(x => Regex.Match(Message, x).Success);

			return hardBounce ? BounceType.HardBounce : BounceType.NotClassified;
		}
	}
}