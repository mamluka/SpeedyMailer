using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Quartz;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities.Extentions;

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
			var serviceSettings = container.Get<ServiceSettings>();

			var st = new Stopwatch();

			pipelines.BeforeRequest.AddItemToEndOfPipeline(x => StartMeasuring(st, x));
			pipelines.AfterRequest.AddItemToEndOfPipeline(x => LogResponse(st, x, logger));
			pipelines.AfterRequest.AddItemToEndOfPipeline(x => HyperMedia(x, serviceSettings));
			pipelines.OnError.AddItemToEndOfPipeline((x, ex) => LogErrors(x, logger, ex));

			base.ApplicationStartup(container, pipelines);
		}

		private static Response StartMeasuring(Stopwatch st, NancyContext x)
		{
			st.Start();
			return x.Response;
		}

		private void HyperMedia(NancyContext x, ServiceSettings settings)
		{
			if (!((string)x.Request.Query["hypermedia"]).HasValue())
				return;

			var contents = x.Response.Contents;
			x.Response.Contents = stream =>
				{
					var memStream = new MemoryStream();
					contents(memStream);
					memStream.Position = 0;

					using (var reader = new StreamReader(memStream))
					{
						var responseContent = reader.ReadToEnd();
						responseContent = Regex.Replace(responseContent, "\"(\\w+?/\\d{1,})\"", match => "\"" + settings.BaseUrl + "/sys/by-id?id=" + match.Groups[1].Value + "\"");

						var sw = new StreamWriter(stream, Encoding.Default);
						sw.Write(responseContent);
						sw.Flush();
					}
				};
		}

		private static Response LogErrors(NancyContext x, Logger logger, Exception ex)
		{
			var requestContent = "";
			using (var reader = new StreamReader(x.Request.Body))
			{
				requestContent = reader.ReadToEnd();
			}

			logger.Error("[{0}] The request path was {1} \n with body: {2} \n exception: {3}", x.Request.Method, x.Request.Url, requestContent, ex);

			return x.Response;
		}

		private static void LogResponse(Stopwatch st, NancyContext x, Logger logger)
		{
			st.Stop();
			st.Reset();
			var stream = new MemoryStream();

			var responseContent = "";
			if (x.Response != null)
			{
				x.Response.Contents(stream);
				stream.Position = 0;
				using (var reader = new StreamReader(stream))
				{
					responseContent = reader.ReadToEnd();
				}
			}

			var requestContent = "";
			using (var reader = new StreamReader(x.Request.Body))
			{
				requestContent = reader.ReadToEnd();
			}

			logger.Info("[{0}] The request path was {1} \n with body: {2} \n with response: {3} \n request took: {4} ms", x.Request.Method, x.Request.Url, requestContent, responseContent,
						st.ElapsedMilliseconds);
		}
	}
}