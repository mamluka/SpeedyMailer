using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Raven.Client;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Modules
{
	public class HeuristicsModule : NancyModule
	{
		public HeuristicsModule(IDocumentStore documentStore)
			: base("/heuritics")
		{
			Get["/delivery"] = x =>
								   {
									   using (var session = documentStore.OpenSession())
									   {
										   var rules = session
											   .Query<DeliverabilityClassificationRules>()
											   .FirstOrDefault() ?? new DeliverabilityClassificationRules();

										   return Response.AsJson(rules);
									   }
								   };

			Post["/delivery"] = x =>
									{
										var model = this.Bind<ServiceEndpoints.Heuristics.SetDeliveryRules>();

										using (var session = documentStore.OpenSession())
										{
											var rules = session.Query<DeliverabilityClassificationRules>()
												.Customize(customization=> customization.WaitForNonStaleResults())
												.SingleOrDefault() ?? new DeliverabilityClassificationRules();

											rules.Rules = model.DeliverabilityClassificationRules.Rules;

											session.Store(rules);
											session.SaveChanges();

											return Response.AsText("OK");
										}
									};
		}
	}
}
