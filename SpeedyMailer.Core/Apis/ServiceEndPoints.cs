using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Core.Apis
{
	public class ServiceEndpoints
	{
		public class RegisterDrone : ApiCall<RegisterDrone.Request>
		{
			public RegisterDrone():base("drones/register") {}

			public class Request
			{
				public string Identifier { get; set; }
			}
		}

		public class FetchFragment : ApiCall
		{
			public FetchFragment() : base("fragments/fetch")
			{}

			public class Response
			{
				public CreativeFragment CreativeFragment { get; set; }
			}
		}
	}
}