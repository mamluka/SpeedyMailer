namespace SpeedyMailer.Core.Domain.Drones
{
	public class Drone
	{
		public string Id { get; set; }
		public string Hostname { get; set; }
		public DroneStatus Status { get; set; }
	}
}
