using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Ninject;
using Raven.Client;
using Raven.Client.Linq;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Storage.Indexes;

namespace SpeedyMailer.Master.Service.Modules
{
	public class AdminModule : NancyModule
	{
		public AdminModule(ServiceSettings serviceSettings,
			IKernel kernel,
			IDocumentStore documentStore
			)
			: base("/admin")
		{
			Get["/settings"] = x =>
								   {

									   ContainerBootstrapper.ReloadAllStoreSettings(kernel);

									   return Response.AsJson(new ServiceEndpoints.Admin.GetRemoteServiceConfiguration.Response
															   {
																   ServiceBaseUrl = serviceSettings.BaseUrl
															   });
								   };
			Get["/info"] = x => Response.AsJson("OK");

			Get["/store/domain-counter"] = x =>
											{
												var listId = (string)Request.Query["listId"];
												var group = (string)Request.Query["group"];
												using (var session = documentStore.OpenSession())
												{
													RavenQueryStatistics ravenQueryStatistics;
													return Response.AsJson(session.Query<Contacts_DomainGroupCounter.ReduceResult, Contacts_DomainGroupCounter>()
														.Where(result => result.ListId == listId && result.DomainGroup == group)
														.Statistics(out ravenQueryStatistics)
														.Select(result => new { result.Count, result.DomainGroup, ravenQueryStatistics.IsStale }));
												}
											};

			Get["/drone-domains"] = x =>
				{
					using (var session = documentStore.OpenSession())
					{
						return
							Response.AsText(string.Join(Environment.NewLine, session
																				 .Query<Drone>()
																				 .ToList()
																				 .Where(drone => drone.LastUpdated > DateTime.UtcNow.AddMinutes(-6))
																				 .Select(drone => drone.Domain)
																				 .ToList()));
					}
				};

			Get["/drone-exceptions"] = x =>
				{
					using (var session = documentStore.OpenSession())
					{
						return
							Response.AsText(string.Join(Environment.NewLine, session
																				 .Query<Drones_Exceptions.ReduceResult, Drones_Exceptions>()
																				 .First(result => result.Group == "All")
																				 .Exceptions.Distinct(new LambdaComparer<string>((m, n) => m.Substring(20) == n.Substring(20)))));
					}
				};

			Get["/change-fragments"] = x =>
				{
					using (var session = documentStore.OpenSession())
					{
						var fragments = session.Query<CreativeFragment>().Where(q => q.CreativeId == "creatives/33").ToList();
						fragments.ForEach(fragment =>
							{
								fragment.HtmlBody =
									"We noticed two issues with your site.  It looks like it is not mobile ready, and only your competition has top placement in Google.  We can change that, and are willing to do it for free.  We offer free mobile website conversions to business who are not mobile ready, and one month of free Google top placement, so businesses can feel what it’s like to take their competitors prospects.<br><br> No gimmick no catch.  We do these things to show results.  Take a moment to see what your site would look like in proper mobile ready form.  Also take away your competitors phone calls.  It’s free.  Stop letting the other guy take all the business.<br><br> Visit <a href=\"http://centracorporation.com\">http://centracorporation.com</a><br> At the bottom of the page is a form.  Fill it out and we will do the rest.<br>We also have live chat support in case you have any questions.<br> Thank,<br> The Team @ Centra<br>";
								session.Store(fragment);
							});

						session.SaveChanges();
					}
					return "OK";
				};
		}
	}
}
