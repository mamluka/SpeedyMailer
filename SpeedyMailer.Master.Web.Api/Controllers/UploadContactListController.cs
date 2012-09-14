using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AttributeRouting.Web.Http;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
    public class UploadContactListController : ApiController
    {
		[GET("/lists/upload")]
		public string Upload()
		{
			return "OK";
		}
    }
}
