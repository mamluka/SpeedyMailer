using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Raven.Client;
using SpeedyMailer.Master.Service.Storage.Indexes;

namespace SpeedyMailer.Master.Service.Modules
{
    public class StatisticsModule : NancyModule
    {
        public StatisticsModule(IDocumentStore documentStore)
            : base("/stats")
        {
            Get["/clicks/{creative}"] = call =>
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var creative = (string)call.creative;
                        var results = session.Query<Creative_ClickActions.ReduceResult, Creative_ClickActions>().Where(x => x.CreativeId == creative);

                        return Response.AsJson(results);
                    }
                };

            Get["/unsubscribes/{creative}"] = call =>
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var creative = (string)call.creative;
                        var results = session.Query<Creative_UnsubscribeRequests.ReduceResult, Creative_UnsubscribeRequests>().Where(x => x.CreativeId == creative);

                        return Response.AsJson(results);
                    }
                };

            Get["/logs/{drone}"] = call =>
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var drone = (string)call.drone;
                        var results = session.Query<Creative_RawLogs.ReduceResult, Creative_RawLogs>().Where(x => x.DroneId == drone);

                        return Response.AsJson(results);
                    }
                };

            Get["/sent/{creative}"] = call =>
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var creative = (string)call.creative;
                        var results = session.Query<Creative_SentEmails.ReduceResult, Creative_SentEmails>().Where(x => x.CreativeId == creative);

                        return Response.AsJson(results);
                    }
                };

            Get["/bounced/{creative}"] = call =>
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var creative = (string)call.creative;
                        var results = session.Query<Creative_BouncedEmails.ReduceResult, Creative_BouncedEmails>().Where(x => x.CreativeId == creative);

                        return Response.AsJson(results);
                    }
                };

            Get["/deferred/{creative}"] = call =>
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var creative = (string)call.creative;
                        var results = session.Query<Creative_DeferredEmails.ReduceResult, Creative_DeferredEmails>().Where(x => x.CreativeId == creative);

                        return Response.AsJson(results);
                    }
                };

            Get["/sending-report/{creative}"] = call =>
                {
                    using (var session = documentStore.OpenSession())
                    {
                        var creative = (string)call.creative;
                        var results = session.Query<Creative_SendingReport.ReduceResult, Creative_SendingReport>().Where(x => x.CreativeId == creative);

                        return Response.AsJson(results);
                    }
                };

        }
    }
}
