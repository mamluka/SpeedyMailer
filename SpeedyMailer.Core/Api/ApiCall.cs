using System;

namespace SpeedyMailer.Core.Api
{
	public class ApiCall
	{
		public object BoxedParameters { get; set; }
		public virtual string Endpoint { get; set; }
	}

	public class ApiCall<T> : ApiCall, IApiHost where T : new()
	{
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