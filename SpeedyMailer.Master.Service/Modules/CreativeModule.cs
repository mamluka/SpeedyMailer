using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Modules
{
	public class CreativeModule : NancyModule
	{
		public CreativeModule(Framework framework, CreativeFragmentSettings creativeFragmentSettings, AddCreativeCommand addCreativeCommand)
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

			Post["/save"] = call =>
								{
									var model = this.Bind<ServiceEndpoints.SaveCreative>();

									addCreativeCommand.Body = model.Body;
									addCreativeCommand.Lists = new List<string> { model.ListId };
									addCreativeCommand.Subject = model.Subject;
									addCreativeCommand.UnsubscribeTemplateId = model.UnsubscribeTemplateId;
									addCreativeCommand.DealUrl = model.DealUrl;

									return Response.AsJson(new ApiStringResult { Result = addCreativeCommand.Execute() });
								};
		}
	}
}
