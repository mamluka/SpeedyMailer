using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpeedyMailer.Core.Rules;

namespace SpeedyMailer.Core.Apis
{
	public partial class ServiceEndpoints
	{
		public class Rules
		{
			public class AddIntervalRules:ApiCall
			{
				public AddIntervalRules() : base("/rules/interval")
				{
					CallMethod = RestMethod.Post;
				}

				public List<IntervalRule> IntervalRules { get; set; }
			}
		}
	}
}
