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
			public class Save:ApiCall
			{
				public Save() : base("/rules")
				{
					CallMethod = RestMethod.Post;
				}

				public RuleAction Action { get; set; }
				public What What { get; set; }
			}
		}
	}
}
