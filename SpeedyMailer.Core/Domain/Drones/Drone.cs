using System;

namespace SpeedyMailer.Core.Domain.Drones
{
	public class Drone
	{
		public string Id { get; set; }
		public string BaseUrl { get; set; }
		public DroneStatus Status { get; set; }
		public DateTime LastUpdated { get; set; }
	}
}
