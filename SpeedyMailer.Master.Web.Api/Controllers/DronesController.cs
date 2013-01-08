using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
    public class DronesController : ApiController
    {
	    private readonly SpeedyMailer.Core.Apis.Api _api;

	    public DronesController(SpeedyMailer.Core.Apis.Api api)
	    {
		    _api = api;
	    }

	    [GET("/drones")]
        public IEnumerable<Drone> GetDrones()
	    {
		    return _api.Call<ServiceEndpoints.Drones.Get, List<Drone>>();
	    }

    }
}
