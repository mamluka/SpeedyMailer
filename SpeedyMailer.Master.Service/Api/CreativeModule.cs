using Nancy;
using Nancy.ModelBinding;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Api
{
	public class CreativeModule : NancyModule
	{
		public CreativeModule(Framework framework, ICreativeFragmentSettings creativeFragmentSettings)
			: base("/creative")
		{
			Post["/add"] = call =>
							  {
								  var model = this.Bind<CreativeApi.Add.Request>();
								  framework.SaveAndExecuteTask(new CreateCreativeFragmentsTask
												  {
													  CreativeId = model.CreativeId,
													  RecipientsPerFragment = creativeFragmentSettings.RecipientsPerFragment
												  });

								  return "sdfsdf";
							  };
		}
	}
}
