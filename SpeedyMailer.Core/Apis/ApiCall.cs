using System;
using System.Web.UI.WebControls.WebParts;

namespace SpeedyMailer.Core.Apis
{
	public class ApiCall
	{
		protected ApiCall(string endpoint)
		{
			Endpoint = endpoint;
		}
		public object BoxedParameters { get; set; }
		public string Endpoint { get; set; }
	}

	public abstract class ApiCall<T> : ApiCall where T : new()
	{
		protected ApiCall(string endpoint) : base(endpoint)
		{ }

		public  T Parameters;

		public void WithParameters(Action<T> action)
		{
			var parameters = new T();
			action(parameters);
			BoxedParameters = parameters;
			Parameters = parameters;
		}
	}
}