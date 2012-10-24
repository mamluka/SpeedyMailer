using System.Collections.Generic;

namespace SpeedyMailer.Core.Rules
{
	public class IntervalRule
	{
		public List<string> Conditons { get; set; }
		public int Interval { get; set; }

		public string Group { get; set; }
	}
}