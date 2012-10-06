using System;
using System.Collections.Generic;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Client.Linq;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Modules
{
	public class CreativeModule : NancyModule
	{
		public CreativeModule(Framework framework, IDocumentStore documentStore, CreativeFragmentSettings creativeFragmentSettings, AddCreativeCommand addCreativeCommand)
			: base("/creative")
		{
			Post["/send"] = call =>
							  {
								  var model = this.Bind<ServiceEndpoints.Creative.Send>();
								  framework.ExecuteTask(new CreateCreativeFragmentsTask
												  {
													  CreativeId = model.CreativeId,
													  RecipientsPerFragment = creativeFragmentSettings.RecipientsPerFragment
												  });

								  return Response.AsText("OK");
							  };

			Post["/save"] = call =>
								{
									var model = this.Bind<ServiceEndpoints.Creative.SaveCreative>();

									addCreativeCommand.Body = model.Body;
									addCreativeCommand.Lists = new List<string> { model.ListId };
									addCreativeCommand.Subject = model.Subject;
									addCreativeCommand.UnsubscribeTemplateId = model.UnsubscribeTemplateId;
									addCreativeCommand.DealUrl = model.DealUrl;

								    var creativeId = addCreativeCommand.Execute();

								    return Response.AsJson(new ApiStringResult { Result = creativeId });
								};

			Get["/fragments"] = call =>
									{
										using (var session = documentStore.OpenSession())
										{

											session.Advanced.UseOptimisticConcurrency = true;

											while (true)
											{
												try
												{
													var creativeFragment = session.Query<CreativeFragment>()
													.Customize(x => x.WaitForNonStaleResults())
													.Where(x => x.Status == FragmentStatus.Pending)
													.ToList()
													.FirstOrDefault();

													if (creativeFragment == null)
														return null;

													creativeFragment.Status = FragmentStatus.Sending;

													session.Store(creativeFragment);
													session.SaveChanges();
													return Response.AsJson(creativeFragment);

												}
												catch (ConcurrencyException ex)
												{
													return null;
												}
											}
										}
									};
		}
	}
}
