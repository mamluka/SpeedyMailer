using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Core.Apis
{
	partial class ServiceEndpoints
	{
		public class Heuristics
		{
			public class GetDeliveryRules:ApiCall
			{
				public GetDeliveryRules() : base("/heuritics/delivery")
				{
					CallMethod = RestMethod.Get;
				}
			}

			public class SetDeliveryRules:ApiCall
			{
				public DeliverabilityClassificationRules DeliverabilityClassificationRules { get; set; }

				public SetDeliveryRules()
					: base("/heuritics/delivery")
				{
					CallMethod = RestMethod.Post;
				}
			}
		}
	}
}
