using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Nancy.Diagnostics;
using Ninject;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class RestCallTestingBootstrapper<TEndpoint, TResponse> : NinjectNancyBootstrapper where TResponse : class
	{
		private readonly string _baseAndEndpoint;
		private readonly TResponse _response;

		public RestCallTestingBootstrapper(string baseAndEndpoint, TResponse response)
		{
			_response = response;
			_baseAndEndpoint = baseAndEndpoint;
		}

		protected override void RegisterRequestContainerModules(IKernel container, IEnumerable<ModuleRegistration> moduleRegistrationTypes)
		{
			container.Bind<NancyModule>()
				.ToConstant(new RestCallTestingModule<TEndpoint, TResponse>(_baseAndEndpoint, _response))
				.Named(GetModuleKeyGenerator().GetKeyForModuleType(typeof(RestCallTestingModule<TEndpoint, TResponse>)));
		}

		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				return NancyInternalConfiguration.WithOverrides(
					c => c.Serializers.Insert(0, typeof(NancyJsonNetSerializer)));
			}
		}
	}
}