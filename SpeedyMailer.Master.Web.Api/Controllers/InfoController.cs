using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Routing;
using AttributeRouting.Web.Http;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Master.Web.Api.App_Start;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class InfoController : ApiController
	{
		private readonly SettingsProvider _settingsProvider;

		public InfoController(SettingsProvider settingsProvider)
		{
			_settingsProvider = settingsProvider;
		}

		[GET("/info"), HttpGet]
		public dynamic Info()
		{

			return "OK";
		}

		[GET("/get-settings"), HttpGet]
		public IList<Dictionary<string,object>> Settings()
		{
			return _settingsProvider.Settings();
		}
	}
}
