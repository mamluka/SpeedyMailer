using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Nancy;
using Raven.Client;
using SpeedyMailer.Core.Domain.Contacts;
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

			Get["/clickers"] = _ =>
				{
					using (var session = documentStore.OpenSession())
					{
						var clicks = session.Query<Creative_ClickActions.ReduceResult, Creative_ClickActions>()
							.Customize(x => x.Include<Contact>(contact => contact.Id))
							.ToList()
							.SelectMany(x => x.ClickedBy)
							.ToList();

						return Response.AsJson(new
							{
								TotalClicks = clicks.Count,
								Contacts = clicks.Select(x => session.Load<Contact>(x.Contactid).Email).ToList()
							});
					}
				};

			Get["/unsubscribers"] = _ =>
				{
					using (var session = documentStore.OpenSession())
					{
						var unsubscribers = session.Query<Creative_UnsubscribeRequests.ReduceResult, Creative_UnsubscribeRequests>()
							.Customize(x => x.Include<Contact>(contact => contact.Id))
							.ToList()
							.SelectMany(x => x.UnsubscribeRequests)
							.ToList();

						return Response.AsJson(new
							{
								TotalUnsubscribes = unsubscribers.Count,
								Contacts = unsubscribers.Select(x => session.Load<Contact>(x).Email).ToList()
							});
					}
				};


			Get["/true-clickers"] = _ =>
				{
					using (var session = documentStore.OpenSession())
					{
						var clicks = session.Query<Creative_ClickActions.ReduceResult, Creative_ClickActions>()
											.Customize(x => x.Include<Contact>(contact => contact.Id))
											.ToList()
											.SelectMany(x => x.ClickedBy)
											.Select(x => x.Contactid)
											.ToList();

						var unsubscribers = session.Query<Creative_UnsubscribeRequests.ReduceResult, Creative_UnsubscribeRequests>()
												   .Customize(x => x.Include<Contact>(contact => contact.Id))
												   .ToList()
												   .SelectMany(x => x.UnsubscribeRequests)
												   .ToList();


						var trueClickers = clicks.Except(unsubscribers);

						return Response.AsJson(new
							{
								TotalTrueClicks = unsubscribers.Count,
								Contacts = trueClickers.Select(x => session.Load<Contact>(x).Email).ToList()
							});
					}
				};
		}
	}
}
