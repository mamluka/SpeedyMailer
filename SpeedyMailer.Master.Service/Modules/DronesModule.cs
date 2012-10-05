using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Raven.Client;
using SpeedyMailer.Core.Apis;
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

	        Post["/"] = x =>
		        {
			        var model = this.Bind<ServiceEndpoints.RegisterDrone>();
			        using (var session = _documentStore.OpenSession())
			        {
				        session.Store(new Drone
					        {
						        BaseUrl = model.BaseUrl,
								Id = model.Identifier,
								LastUpdated = DateTime.Parse(model.LastUpdate)
					        });
						session.SaveChanges();
			        }
			        return Response.AsText("OK");
		        };
        }
    }
}
