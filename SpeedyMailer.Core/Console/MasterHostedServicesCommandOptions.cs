using CommandLine;

namespace SpeedyMailer.Core.Console
{
	public class MasterHostedServicesCommandOptions : CommandLineOptionsBase
	{
		[Option("b", "base-url", DefaultValue = @"http://localhost:9852", HelpText = "The base url of the service to register the drone with")]
		public string BaseUrl { get; set; }
	}
}