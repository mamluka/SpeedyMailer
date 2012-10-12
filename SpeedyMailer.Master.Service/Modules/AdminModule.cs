using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Ninject;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Master.Service.Modules
{
	public class AdminModule : NancyModule
	{
		public AdminModule(ServiceSettings serviceSettings, IKernel kernel)
			: base("/admin")
        {
			Get["/settings"] = x =>
				                   {

					                   ContainerBootstrapper.ReloadAllStoreSettings(kernel);

					                   return Response.AsJson(new ServiceEndpoints.Admin.GetRemoteServiceConfiguration.Response
							                                   {
								                                   ServiceBaseUrl = serviceSettings.BaseUrl
							                                   });
				                   };
            Get["/info"] = x => Response.AsJson("OK");
        }
	}
}
