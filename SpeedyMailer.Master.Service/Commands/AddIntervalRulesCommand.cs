using System.Collections.Generic;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Rules;

namespace SpeedyMailer.Master.Service.Commands
{
	public class AddIntervalRulesCommand:Command
	{
		public IEnumerable<IntervalRule> Rules { get; set; }

		public override void Execute()
		{
			
		}
	}
}