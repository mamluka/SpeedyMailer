using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Raven.Client;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Storage.Indexes;

namespace SpeedyMailer.Master.Service.Modules
{
	public class StatisticsModule : NancyModule
	{
		public StatisticsModule(IDocumentStore documentStore)
			: base("/stats")
		{
			Get["/clicks"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var creativeId = (string)Request.Query["creativeid"];
						var results = session.Query<Creative_ClickActions.ReduceResult, Creative_ClickActions>().Where(x => x.CreativeId == creativeId);

						return Response.AsJson(results);
					}
				};

			Get["/unsubscribes/"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var creativeId = (string)Request.Query["creativeid"];
						var results = session.Query<Creative_UnsubscribeRequests.ReduceResult, Creative_UnsubscribeRequests>().Where(x => x.CreativeId == creativeId);

						return Response.AsJson(results);
					}
				};

			Get["/raw-logs/{drone}"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var drone = (string)call.drone;
						var results = session.Query<Creative_RawLogs.ReduceResult, Creative_RawLogs>().Where(x => x.DroneId == drone).ToList();

						return Response.AsJson(results);
					}
				};

			Get["/logs/{drone}"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var drone = (string)call.drone;
						var results = session.Query<Creative_RawLogs.ReduceResult, Creative_RawLogs>().Where(x => x.DroneId == drone).ToList();

						var lines = results.SelectMany(x => x.RawLogs).Select(x => x.Time.ToLongTimeString() + " " + x.Message).ToList();
						return Response.AsText(string.Join(Environment.NewLine, lines));
					}
				};

			Get["/sent/"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var creativeId = (string)Request.Query["creativeid"];
						var results = session.Query<Creative_SentEmails.ReduceResult, Creative_SentEmails>().Where(x => x.CreativeId == creativeId);

						return Response.AsJson(results);
					}
				};

			Get["/bounced"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var creativeId = (string)Request.Query["creativeid"];
						var results = session.Query<Creative_BouncedEmails.ReduceResult, Creative_BouncedEmails>().Where(x => x.CreativeId == creativeId);

						return Response.AsJson(results);
					}
				};

			Get["/deferred"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var creativeId = (string)Request.Query["creativeid"];
						var results = session.Query<Creative_DeferredEmails.ReduceResult, Creative_DeferredEmails>().Where(x => x.CreativeId == creativeId);

						return Response.AsJson(results);
					}
				};

			Get["/sending-report"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var creativeId = (string)Request.Query["creativeid"];
						var results = session.Query<Creative_SendingReport.ReduceResult, Creative_SendingReport>().Where(x => x.CreativeId == creativeId);

						return Response.AsJson(results);
					}
				};

			Get["/senitized-sending-report"] = call =>
				{
					using (var session = documentStore.OpenSession())
					{
						var creativeId = (string)Request.Query["creativeid"];

						var sent = session.Query<Creative_SentEmails.ReduceResult, Creative_SentEmails>().Where(x => x.CreativeId == creativeId).ToList();
						var bounced = session.Query<Creative_BouncedEmails.ReduceResult, Creative_BouncedEmails>().Where(x => x.CreativeId == creativeId).ToList();
						var deferred = session.Query<Creative_DeferredEmails.ReduceResult, Creative_DeferredEmails>().Where(x => x.CreativeId == creativeId).ToList();

						var sanitizedSends = sent.SelectMany(result => result.Sends.Distinct(new LambdaComparer<GenericMailEvent>((x, y) => x.Recipient == y.Recipient))).ToList();
						var sanitizedBounces = bounced.SelectMany(result => result.Bounced.Distinct(new LambdaComparer<GenericMailEvent>((x, y) => x.Recipient == y.Recipient))).ToList();
						var sanitizedDeferres = deferred.SelectMany(result => result.Deferred.Distinct(new LambdaComparer<GenericMailEvent>((x, y) => x.Recipient == y.Recipient))).ToList();

						return Response.AsJson(new
							{
								Sent = sanitizedSends,
								Bounced = sanitizedBounces,
								Deferred = sanitizedDeferres,
								TotalSent = sanitizedSends.Count,
								TotalBounces = sanitizedBounces.Count,
								TotalDeferres = sanitizedDeferres.Count
							});
					}
				};

		}
	}
}
