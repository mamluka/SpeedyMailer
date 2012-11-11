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
			public class Delivery:ApiCall
			{
				public Delivery() : base("/heuritics/delivery")
				{
					CallMethod = RestMethod.Get;
				}
			}
		}
	}
}
