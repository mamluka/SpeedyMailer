using SpeedyMailer.Core.Apis;

namespace SpeedyMailer.Core.Domain.Drones
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

			public class FlushPackages:ApiCall
			{
				public FlushPackages()
					: base("/admin/flush-unprocessed-packages")
				{
					CallMethod = RestMethod.Post;
				}
			}
		}
	}
}