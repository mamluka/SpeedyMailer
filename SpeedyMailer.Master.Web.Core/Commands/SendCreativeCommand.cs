using RestSharp;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Master.Web.Core.Commands
{
	public class SendCreativeCommand : ApiCommand
	{
		private readonly Api _api;
		public string CreativeId { get; set; }

		public SendCreativeCommand(Api api)
		{
			_api = api;
		}

		public override ApiResult Execute()
		{
			_api.Call<CreativeEndpoint.Add>()
				.WithParameters(x => x.CreativeId = CreativeId)
				.Post();

			return new ApiResult();
		}
	}

	public class FaultTolerantResponse
	{
		public bool ExceptionOccured;
		public string ExceptionMessage { get; set; }
		public string ExceptionName { get; set; }
	}

	public class FaultTolerantResponse<T> : FaultTolerantResponse
	{
		public T Model;
	}
}