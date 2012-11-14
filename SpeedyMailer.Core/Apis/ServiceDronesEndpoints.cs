using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

			public class GetDnsblData:ApiCall
			{
				public GetDnsblData() : base("/drones/dnsbl")
				{
					CallMethod = RestMethod.Get;
				}
			}
		}
	}
}
