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
											   .Query<UnDeliveredMailClassificationHeuristicsRules>()
											   .FirstOrDefault() ?? new UnDeliveredMailClassificationHeuristicsRules();

										   return Response.AsJson(rules);
									   }
								   };

			Post["/delivery"] = x =>
									{
										var model = this.Bind<ServiceEndpoints.Heuristics.SetDeliveryRules>();

										using (var session = documentStore.OpenSession())
										{
											var rules = session.Load<UnDeliveredMailClassificationHeuristicsRules>(model.Rules.Id);

											rules.HardBounceRules = model.Rules.HardBounceRules;
											rules.IpBlockingRules = model.Rules.IpBlockingRules;

											session.Store(model.Rules);
											session.SaveChanges();

											return Response.AsText("OK");
										}
									};
		}
	}
}
