using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Raven.Client;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Master.Service.Modules
{
    public class DronesModule : NancyModule
    {
        private readonly IDocumentStore _documentStore;

        public DronesModule(IDocumentStore documentStore)
            : base("/drones")
        {
            _documentStore = documentStore;

            Post["/register"] = x =>
                                    {
                                        using (var session = _documentStore.OpenSession())
                                        {
                                            session.Store(new Drone
                                                              {
                                                                  Hostname = ""
                                                              });
                                        }
                                        return Response.AsText("OK");
                                    };
        }
    }
}
