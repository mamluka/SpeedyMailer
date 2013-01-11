using System;
using System.Diagnostics;
using System.IO;
using NLog;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Quartz;

namespace SpeedyMailer.Core.Container
{
	public class NancyContainerBootstrapper : NinjectNancyBootstrapper
	{
		private readonly Action<IKernel> _action;
		private readonly IScheduler _scheduler;

		public NancyContainerBootstrapper(Action<IKernel> action, IScheduler scheduler)
		{
			_scheduler = scheduler;
			_action = action;
		}

		protected override void ConfigureApplicationContainer(IKernel existingContainer)
		{
			_action(existingContainer);
			existingContainer.Rebind<IScheduler>().ToConstant(_scheduler);
		}

		protected override NancyInternalConfiguration InternalConfiguration
		{
			get
			{
				return NancyInternalConfiguration
					.WithOverrides(c => c.Serializers.Insert(0, typeof(NancyJsonNetSerializer)))
					.WithIgnoredAssembly(asm => !asm.FullName.StartsWith("SpeedyMailer", StringComparison.InvariantCultureIgnoreCase));
			}
		}

		protected override void ApplicationStartup(IKernel container, IPipelines pipelines)
		{
			var logger = LogManager.GetLogger("SpeedyMailer.Nancy.Request");

			var st = new Stopwatch();

			pipelines.BeforeRequest.AddItemToEndOfPipeline(x =>
				{
					st.Start();
					return x.Response;
				});

			pipelines.AfterRequest.AddItemToEndOfPipeline(x =>
				{
					st.Stop();
					var stream = new MemoryStream();

					x.Response.Contents(stream);

					stream.Position = 0;
					var responseContent = "";
					using (var reader = new StreamReader(stream))
					{
						responseContent = reader.ReadToEnd();
					}

					var requestContent = "";
					using (var reader = new StreamReader(x.Request.Body))
					{
						requestContent = reader.ReadToEnd();
					}

					logger.Info("[{0}] The request path was {1} \n with body: {2} \n with response: {3} \n request took: {4} ms", x.Request.Method, x.Request.Url, requestContent, responseContent, st.ElapsedMilliseconds);
				});

			base.ApplicationStartup(container, pipelines);
		}
	}
}