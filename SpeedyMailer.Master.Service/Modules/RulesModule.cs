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
using SpeedyMailer.Master.Service.Commands;

namespace SpeedyMailer.Master.Service.Modules
{
	public class RulesModule:NancyModule
	{
		public RulesModule(AddIntervalRulesCommand addIntervalRulesCommand):base("/rules")
		{
			Post["/interval"] = call =>
				                    {
					                    var model = this.Bind<ServiceEndpoints.Rules.AddIntervalRules>();

					                    addIntervalRulesCommand.Rules = model.IntervalRules;
										addIntervalRulesCommand.Execute();

					                    return Response.AsText("OK");
				                    };
		}
	}
}
