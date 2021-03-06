﻿using System;
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

			Get["/test"] = x =>
				{
					return Response.AsJson(new
						{
							CreativeId = "creatives/1",
							Sex = "contacts/1"
						});
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

			Get["/ex"] = _ =>
				{
					throw new Exception("test");
				};
		}
	}
}
