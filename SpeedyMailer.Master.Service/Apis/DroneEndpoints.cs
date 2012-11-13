using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Master.Service.Apis
{
	public class DroneEndpoints
	{
		public class Admin
		{
			public class Wakeup : ApiCall
			{
				public Wakeup()
					: base("/manage/wakeup")
				{
					CallMethod = RestMethod.Post;
				}

				public class Response
				{
					public DroneStatus DroneStatus { get; set; }
				}
			}

			public class FireTask : ApiCall
			{
				public string Job { get; set; }

				public FireTask()
					: base("/admin/fire-task/{job}")
				{
					CallMethod = RestMethod.Get;
				}
			}
		}
	}
}