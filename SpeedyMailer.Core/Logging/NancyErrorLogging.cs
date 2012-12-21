using System;
using NLog;
using Nancy;
using Nancy.ErrorHandling;
using LogManager = NLog.LogManager;

namespace SpeedyMailer.Core.Logging
{
	public class NancyErrorLogging : IErrorHandler
	{
		private readonly Logger _logger = LogManager.GetLogger(typeof(NancyErrorLogging).FullName);

		public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
		{
			return statusCode == HttpStatusCode.InternalServerError;
		}

		public void Handle(HttpStatusCode statusCode, NancyContext context)
		{
			object errorObject;
			context.Items.TryGetValue(NancyEngine.ERROR_EXCEPTION, out errorObject);
			var error = errorObject as Exception;

			_logger.ErrorException("Nancy Unhandled error:", error);
		}
	}
}
