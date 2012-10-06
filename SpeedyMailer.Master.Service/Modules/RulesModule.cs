using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Raven.Client;
using SpeedyMailer.Core.Apis;

namespace SpeedyMailer.Master.Service.Modules
{
	public class RulesModule:NancyModule
	{
		public RulesModule(IDocumentStore documentStore):base("/rules")
		{
			Post["/interval"] = call =>
				                    {
//					                    var reader = new StreamReader(Request.Body);
//					                    var line = "";
//										while ((line = reader.ReadLine()) != null)
//										{
//											Trace.WriteLine(line); // Write to console.
//										}

					                    var model = this.Bind<ServiceEndpoints.Rules.AddIntervalRules>();

					                    using (var session = documentStore.OpenSession())
					                    {
						                    foreach (var intervalRule in model.IntervalRules)
						                    {
							                    session.Store(intervalRule);
						                    }

											session.SaveChanges();
					                    }

					                    return Response.AsText("OK");
				                    };
		}
	}
}
