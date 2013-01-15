﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using AttributeRouting.Web.Http;
using Renci.SshNet;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Master.Web.Api.Controllers
{
	public class DronesController : ApiController
	{
		private readonly SpeedyMailer.Core.Apis.Api _api;
		private const string ChefHost = "173.224.209.25";

		public DronesController(SpeedyMailer.Core.Apis.Api api)
		{
			_api = api;
		}

		[GET("/drones")]
		public IEnumerable<SlimDrone> GetDrones()
		{
			return _api.Call<ServiceEndpoints.Drones.Get, List<SlimDrone>>();
		}

		[POST("/drones/deploy")]
		public string Deploy(Drone drone)
		{
			using (var ssh = new SshClient(drone.Id, "root", "0953acb"))
			{
				ssh.Connect();
				var cmd = ssh.RunCommand("chef-client");   //  very long list 
				ssh.Disconnect();

				if (cmd.ExitStatus > 0)
					return cmd.Result.Replace("\n", "<br>");

				return "OK";
			}
		}

		[POST("/drones/bootstrap")]
		public string Bootstrap(Drone drone)
		{
			using (var ssh = new SshClient(ChefHost, "root", "0953acb"))
			{
				ssh.Connect();
				var cmd = ssh.RunCommand(string.Format("knife bootstrap {0} -x root -P 0953acb --sudo -N {1} --run-list speedymailer-drone -E xomixfuture", drone.Id, Guid.NewGuid().ToString().Replace("-", "")));   //  very long list 
				ssh.Disconnect();

				return cmd.Result.Replace("\n", "<br>");
			}
		}
		
		[POST("/drones/deploy/all")]
		public string DeployAll()
		{
			var drones = _api.Call<ServiceEndpoints.Drones.Get, List<Drone>>();
			return "OK";
		}
	}
}