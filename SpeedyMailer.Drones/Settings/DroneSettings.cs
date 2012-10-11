using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Settings
{
	public class DroneSettings
	{
		public virtual string Identifier { get; set; }
		public virtual string BaseUrl { get; set; }

		[Default("mongodb://localhost:27027/drone?safe=true")]
		public virtual string StoreHostname { get; set; }
	}
}