using SpeedyMailer.Core.Apis;

namespace SpeedyMailer.Core.Protocol
{
	public class CreativeEndpoint
	{
		public class Add : ApiCall
		{
			public Add() : base("/creative/add")
			{
				CallMethod = RestMethod.Post;
			}

			public string CreativeId { get; set; }

			public class Response
			{

			}
		}
	}
}