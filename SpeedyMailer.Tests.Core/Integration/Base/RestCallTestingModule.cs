using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.Bootstrappers.Ninject;
using Nancy.ModelBinding;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class RestCallTestingModule<TEndpoint, TResponse> : NancyModule, IDoNotResolveModule where TResponse : class
	{
		public static TEndpoint Model;
		public static bool WasCalled;
		public static IList<string> Files;

		public RestCallTestingModule(string endpoint, TResponse response)
		{
			Get[endpoint] = x => RecordResponse(response);

			Post[endpoint] = x => RecordResponse(response);

			Put[endpoint] = x => RecordResponse(response);
		}

		private Response RecordResponse(TResponse response)
		{
			Model = this.Bind<TEndpoint>();
			Files = Request.Files.Select(file => file.Name).ToList();
			WasCalled = true;

			return response != null ? Response.AsJson(response) : Response.AsText("OK");
		}
	}
}