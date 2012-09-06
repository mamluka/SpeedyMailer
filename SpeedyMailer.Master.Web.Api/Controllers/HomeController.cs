using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AttributeRouting.Web.Mvc;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class HomeController : Controller
	{
		[GET("/")]
		public ActionResult Homepage()
		{
			return View();
		}
	}
}
