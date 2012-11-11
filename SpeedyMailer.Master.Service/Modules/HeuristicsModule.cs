using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Raven.Client;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Master.Service.Modules
{
	public class HeuristicsModule:NancyModule
	{
		public HeuristicsModule(IDocumentStore documentStore):base("/heuritics")
		{
			Get["/delivery"] = x =>
				                   {
					                   using (var session = documentStore.OpenSession())
					                   {
						                   var rules = session.Query<UnDeliveredMailClassificationHeuristicsRules>().FirstOrDefault();
						                   return Response.AsJson(rules);
					                   }
				                   };
		}
	}
}
