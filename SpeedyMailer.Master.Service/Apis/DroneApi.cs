using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Master.Service.Apis
{
	public class DroneApi
	{
		public class Manage
		{
			public class Wakeup : ApiCall
			{
				public Wakeup()
					: base("/manage/wakeup")
				{ }

				public class Response
				{
					public DroneStatus DroneStatus { get; set; }
				}
			}
		}
	}
}