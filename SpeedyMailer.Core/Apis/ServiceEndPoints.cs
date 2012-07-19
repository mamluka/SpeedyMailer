using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Core.Apis
{
	public class ServiceEndpoints
	{
		public class RegisterDrone : ApiCall
		{
			public RegisterDrone():base("drones/register")
			{
				CallMethod = RestMethod.Post;
			}

			public string Identifier { get; set; }
		}

		public class FetchFragment : ApiCall
		{
			public FetchFragment() : base("fragments/fetch")
			{
				CallMethod = RestMethod.Get;
			}

			public class Response
			{
				public CreativeFragment CreativeFragment { get; set; }
			}
		}
	}
}