using Nancy;
using Nancy.ModelBinding;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Modules
{
	public class CreativeModule : NancyModule
	{
		public CreativeModule(Framework framework, CreativeFragmentSettings creativeFragmentSettings)
			: base("/creative")
		{
			Post["/add"] = call =>
							  {
								  var model = this.Bind<CreativeEndpoint.Add>();
								  framework.ExecuteTask(new CreateCreativeFragmentsTask
												  {
													  CreativeId = model.CreativeId,
													  RecipientsPerFragment = creativeFragmentSettings.RecipientsPerFragment
												  });

								  return Response.AsText("OK");
							  };
		}
	}
}
