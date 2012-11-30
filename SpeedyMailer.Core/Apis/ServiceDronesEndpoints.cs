﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Core.Apis
{
	public partial class ServiceEndpoints
	{
		public class Drones
		{
			public class RegisterDrone : ApiCall
			{
				public RegisterDrone()
					: base("/drones")
				{
					CallMethod = RestMethod.Post;
				}

				public string Identifier { get; set; }

				public string BaseUrl { get; set; }

				public string LastUpdate { get; set; }
			}

			public class GetDnsblData : ApiCall
			{
				public GetDnsblData()
					: base("/drones/dnsbl")
				{
					CallMethod = RestMethod.Get;
				}
			}

			public class SendStateSnapshot : ApiCall
			{
				public Drone Drone { get; set; }

				public IList<ReducedMailLogEntry> RawLogs { get; set; }

				public IList<MailSent> MailSent { get; set; }
				public IList<MailBounced> MailBounced { get; set; }
				public IList<MailDeferred> MailDeferred { get; set; }

				public SendStateSnapshot()
					: base("/drones/state-snapshot")
				{
					CallMethod = RestMethod.Post;
				}
			}
		}
	}
}
