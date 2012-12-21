using System;
using System.Collections.Generic;
using SpeedyMailer.Core.Domain.Mail;

namespace SpeedyMailer.Core.Domain.Drones
{
	public class Drone
	{
		public string Id { get; set; }
		public string BaseUrl { get; set; }
		public DroneStatus Status { get; set; }
		public DateTime LastUpdated { get; set; }
		public string Domain { get; set; }

		public IpReputation IpReputation { get; set; }

		public List<DroneException> Exceptions { get; set; }
	}
}
