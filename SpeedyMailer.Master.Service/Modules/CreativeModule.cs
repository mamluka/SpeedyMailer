using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using Nancy;
using Nancy.ModelBinding;
using Raven.Abstractions.Exceptions;
using Raven.Client;
using Raven.Client.Linq;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Modules
{
	public class CreativeModule : NancyModule
	{
		public CreativeModule(Framework framework, IDocumentStore documentStore, AddCreativeCommand addCreativeCommand, Logger logger)
			: base("/creative")
		{
			Post["/send"] = call =>
							  {
								  var model = this.Bind<ServiceEndpoints.Creative.Send>();
								  framework.ExecuteTask(new CreateCreativeFragmentsTask
												  {
													  CreativeId = model.CreativeId,
												  });

								  return Response.AsText("OK");
							  };

			Post["/save"] = call =>
								{
									var model = this.Bind<ServiceEndpoints.Creative.SaveCreative>();

									addCreativeCommand.HtmlBody = model.HtmlBody;
									addCreativeCommand.TextBody = model.TextBody;
									addCreativeCommand.Lists = new List<string> { model.ListId };
									addCreativeCommand.Subject = model.Subject;
									addCreativeCommand.UnsubscribeTemplateId = model.UnsubscribeTemplateId;
									addCreativeCommand.DealUrl = model.DealUrl;
									addCreativeCommand.FromName = model.FromName;
									addCreativeCommand.FromAddressDomainPrefix = model.FromAddressDomainPrefix;

									var creativeId = addCreativeCommand.Execute();

									return Response.AsJson(new ApiStringResult { Result = creativeId });
								};

			Get["/getall"] = call =>
								 {
									 using (var session = documentStore.OpenSession())
									 {
										 return Response.AsJson(session.Query<Creative>().Customize(x => x.WaitForNonStaleResults()).ToList());
									 }
								 };

			Get["/fragments"] = call =>
				{
					var model = this.Bind<ServiceEndpoints.Creative.FetchFragment>();
					using (var session = documentStore.OpenSession())
					{
						var creativeFragmentId = "";
						session.Advanced.UseOptimisticConcurrency = true;

						while (true)
						{
							try
							{
								var creativeFragment = session.Query<CreativeFragment>()
															  .Customize(x => x.WaitForNonStaleResults(TimeSpan.FromMinutes(5)))
															  .Where(x => x.Status == FragmentStatus.Pending)
															  .ToList()
															  .FirstOrDefault();

								if (creativeFragment == null)
								{
									logger.Info("No fragments were found");
									return null;
								}

								creativeFragmentId = creativeFragment.Id;

								creativeFragment.Status = FragmentStatus.Sending;
								creativeFragment.FetchedBy = model.DroneId;
								creativeFragment.FetchedAt = DateTime.UtcNow;

								session.Store(creativeFragment);
								session.SaveChanges();

								logger.Info("creative was found with id {0} it has {1} contacts inside", creativeFragment.Id, creativeFragment.Recipients.Count);
								return Response.AsJson(creativeFragment);

							}
							catch (ConcurrencyException ex)
							{
								logger.ErrorException(string.Format("While fetching creative fragment: {0} we had a cuncurrency error", creativeFragmentId), ex);
							}
						}
					}
				};

			Get["/clone"] = _ =>
				{
					var creativeId = (string)Request.Query["creativeId"];
					var listId = (string)Request.Query["listId"];

					using (var session = documentStore.OpenSession())
					{
						var creative = session.Load<Creative>(creativeId);
						if (creative == null)
							return "Error";

						session.Advanced.Evict(creative);
						creative.Id = null;
						creative.Lists.Clear();
						creative.Lists.Add(listId);

						session.Store(creative);
						session.SaveChanges();

						return Response.AsJson(creative);
					}
				};

			Get["/fragments-status"] = _ =>
				{
					using (var session = documentStore.OpenSession())
					{
						return Response.AsJson(session.Query<Fragments_ByCreative.ReduceResult, Fragments_ByCreative>());
					}
				};

			Post["/cancel"] = _ =>
				{
					var model = this.Bind<ServiceEndpoints.Creative.Cancel>();

					using (var sessionn = documentStore.OpenSession())
					{
						var fragments = sessionn.Query<CreativeFragment>().Where(x => x.CreativeId == model.CreativeId && x.Status == FragmentStatus.Pending).ToList();
						fragments.ForEach(x =>
							{
								x.Status = FragmentStatus.Cancelled;
								sessionn.Store(x);
							});

						sessionn.SaveChanges();
					}

					return "OK";
				};
		}
	}
}
