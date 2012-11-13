using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Core.Apis
{
	partial class ServiceEndpoints
	{
		public class Heuristics
		{
			public class SaveDelivery:ApiCall
			{
				public List<string> HardBounceRules { get; set; }
				public List<string> IpBlockingRules { get; set; }

				public SaveDelivery() : base("/heuritics/delivery")
				{
					CallMethod = RestMethod.Get;
				}
			}
		}
	}
}
