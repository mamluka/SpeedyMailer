using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Master.Service.Modules
{
	public class AdminModule:NancyModule
	{
		private readonly ServiceSettings _serviceSettings;

		public AdminModule(ServiceSettings serviceSettings):base("/admin")
		{
			_serviceSettings = serviceSettings;

			Get["/settings/get"] = x => Response.AsJson(new ServiceEndpoints.GetRemoteServiceSettings.Response
			                                            	{
			                                            		ServiceBaseUrl = _serviceSettings.BaseUrl
			                                            	});
		}
	}
}
