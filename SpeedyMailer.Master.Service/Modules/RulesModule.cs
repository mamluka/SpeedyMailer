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
using SpeedyMailer.Core.Rules;
using SpeedyMailer.Master.Service.Commands;

namespace SpeedyMailer.Master.Service.Modules
{
	public class RulesModule:NancyModule
	{
		public RulesModule(AddIntervalRulesCommand addIntervalRulesCommand,IDocumentStore documentStore):base("/rules")
		{
			Post["/interval"] = call =>
				                    {
					                    var model = this.Bind<ServiceEndpoints.Rules.AddIntervalRules>();

					                    addIntervalRulesCommand.Rules = model.IntervalRules;
										addIntervalRulesCommand.Execute();

					                    return Response.AsText("OK");
				                    };
			
			Get["/interval"] = call =>
				                    {
					                    using (var session = documentStore.OpenSession())
					                    {
						                    return Response.AsJson(session.Query<IntervalRule>());
					                    }
				                    };
		}
	}
}
