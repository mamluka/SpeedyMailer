namespace SpeedyMailer.Core.Apis
{
	public class ServiceApi
	{
		public class RegisterDrone : ApiCall<RegisterDrone.Request>
		{
			public RegisterDrone():base("drones/register") {}

			public class Request
			{
				public string Identifier { get; set; }
			}
		}
	}
}