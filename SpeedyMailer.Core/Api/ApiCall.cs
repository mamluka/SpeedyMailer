using System;

namespace SpeedyMailer.Core.Api
{
	public class ApiCall
	{
		public object BoxedParameters { get; set; }
		public string Endpoint { get; set; }
	}

	public abstract class ApiCall<T> : ApiCall, IApiHost where T : new()
	{
		protected ApiCall(string endpoint)
		{
			Endpoint = endpoint;
		}

		public T Parameters { get; set; }
		public Api ApiContext { get; set; }

		public Api WithParameters(Action<T> action)
		{
			Parameters = new T();
			action(Parameters);
			BoxedParameters = Parameters;
			return ApiContext;
		}
	}
}