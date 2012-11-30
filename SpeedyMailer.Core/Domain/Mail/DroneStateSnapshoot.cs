using System.Collections.Generic;
using SpeedyMailer.Core.Domain.Drones;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class DroneStateSnapshoot
	{
		public Drone Drone { get; set; }

		public IList<ReducedMailLogEntry> RawLogs { get; set; }

		public IList<MailSent> MailSent { get; set; }
		public IList<MailBounced> MailBounced { get; set; }
		public IList<MailDeferred> MailDeferred { get; set; }
	}
}