using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Apis;

namespace SpeedyMailer.Core.Protocol
{
	public class CreativeEndpoint
	{
		public class Add : ApiCall<Add.Request>
		{
			public Add() : base("/creative/add") {}

			public class Request
			{
				public string CreativeId { get; set; }
			}

			public class Response
			{

			}
		}
	}
}