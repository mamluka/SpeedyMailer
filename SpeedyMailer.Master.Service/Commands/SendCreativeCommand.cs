using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Protocol;

namespace SpeedyMailer.Master.Service.Commands
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
			_api.Call<CreativeEndpoint.Send>(x => x.CreativeId = CreativeId);

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