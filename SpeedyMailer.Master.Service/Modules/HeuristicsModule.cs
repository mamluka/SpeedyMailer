using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Raven.Client;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Storage;

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
											   .LoadSingle<DeliverabilityClassificationRules>()
											    ?? new DeliverabilityClassificationRules();

										   return Response.AsJson(rules);
									   }
								   };

			Post["/delivery"] = x =>
									{
										var model = this.Bind<ServiceEndpoints.Heuristics.SetDeliveryRules>();

										using (var session = documentStore.OpenSession())
										{
											var rules = session.LoadSingle<DeliverabilityClassificationRules>()
												?? new DeliverabilityClassificationRules();

											rules.Rules = model.DeliverabilityClassificationRules.Rules;

											session.StoreSingle(rules);
											session.SaveChanges();

											return Response.AsText("OK");
										}
									};
		}
	}
}
