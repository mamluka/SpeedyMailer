using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Core.Apis
{
	public partial class ServiceEndpoints
	{
		public class Admin
		{
			public class GetRemoteServiceSettings : ApiCall
			{
				public GetRemoteServiceSettings()
					: base("/admin/settings")
				{
					CallMethod = RestMethod.Get;
				}

				public class Response
				{
					public string ServiceBaseUrl { get; set; }
				}
			}
		}
	}
}
