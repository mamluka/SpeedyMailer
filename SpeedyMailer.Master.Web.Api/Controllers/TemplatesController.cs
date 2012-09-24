using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
    public class TemplatesController : ApiController
    {
	    private SpeedyMailer.Core.Apis.Api _api;

	    public TemplatesController(SpeedyMailer.Core.Apis.Api api)
	    {
		    _api = api;
	    }

	    [GET("/templates/{templateType}/")]
		public IList<Template> GetTemplates()
	    {
		    return null;
		    //return _api.Call<ServiceEndpoints.CreateUnsubscribeTemplate>()
	    }
    }
}
