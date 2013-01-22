using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nancy;
using Raven.Client;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Storage;
using SpeedyMailer.Core.Utilities.Extentions;
using SpeedyMailer.Master.Service.Storage.Indexes;

namespace SpeedyMailer.Master.Service.Modules
{
	public class AnalyzeModule : NancyModule
	{
		public AnalyzeModule(IDocumentStore documentStore)
			: base("/analyze")
		{
			Get["/uneasy-domains"] = _ =>
				{
					var verbose = ((string)Request.Query["mode"]).HasValue();
					using (var session = documentStore.OpenSession())
					{
						var bounces = session.Query<Creative_AllBounces.ReduceResult, Creative_AllBounces>().SingleOrDefault(x => x.Group == "All").Bounced;

						var rules =
							session.LoadSingle<DeliverabilityClassificationRules>()
								   .Rules
								   .AsParallel()
								   .Where(x => x.Type == Classification.IpBlocking || x.Type == Classification.ContentBlocking)
								   .ToList();

						var uneasyBounces = bounces
							.AsParallel()
							.Where(x => rules.Any(m => Regex.Match(x.Message, m.Condition, RegexOptions.IgnoreCase).Success));

						if (verbose)
							return Response.AsJson(uneasyBounces);

						return Response.AsText(uneasyBounces.Select(x => x.Recipient.GetDomain()).Distinct().Linefy());
					}
				};
		}
	}
}
