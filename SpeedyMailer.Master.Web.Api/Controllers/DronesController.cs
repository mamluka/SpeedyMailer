using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
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
			return _api.Call<ServiceEndpoints.Drones.Get, List<SlimDrone>>()
				.Where(x => x.LastUpdated.ToUniversalTime() > DateTime.UtcNow.AddMinutes(-10))
				.ToList();
		}

		[POST("/drones/deploy")]
		public object Deploy(Drone drone)
		{
			return SendCommandToDrone(drone, "chef-client");
		}

		[POST("/drones/kill")]
		public object Stop(Drone drone)
		{
			return SendCommandToDrone(drone, "/deploy/utils/drone-admin.rb stop");
		}

		private static object SendCommandToDrone(Drone drone, string commandText)
		{
			using (var ssh = new SshClient(drone.Id, "root", "0953acb"))
			{
				ssh.Connect();
				var cmd = ssh.RunCommand(commandText); //  very long list 
				ssh.Disconnect();

				if (cmd.ExitStatus > 0)
					return new
					{
						Drone = drone,
						Data = cmd.Result.Replace("\n", "<br>")
					};

				return new
					{
						Drone = drone,
						Data = "OK"
					};
			}
		}

		[POST("/drones/bootstrap")]
		public object Bootstrap(Drone drone)
		{
			using (var ssh = new SshClient(ChefHost, "root", "0953acb"))
			{
				ssh.Connect();
				var cmd = ssh.RunCommand(string.Format("knife bootstrap {0} -x root -P 0953acb --sudo -N {1} --run-list speedymailer-drone -E xomixfuture", drone.Id, Guid.NewGuid().ToString().Replace("-", "")));   //  very long list 
				ssh.Disconnect();

				return new
				{
					Drone = drone,
					Data = cmd.Result.Replace("\n", "<br>")
				};
			}
		}

		[POST("/drones/deploy/all")]
		public List<object> DeployAll()
		{
			var drones = _api.Call<ServiceEndpoints.Drones.Get, List<Drone>>();
			return drones
				.AsParallel()
				.WithDegreeOfParallelism(4)
				.Select(drone => SendCommandToDrone(drone, "chef-client"))
				.ToList();
		}
	}
}