using System.Collections.Generic;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Drones;
using SpeedyMailer.Core.Domain.Emails;

namespace SpeedyMailer.Core.Domain.Mail
{
	public class DroneStateSnapshoot
	{
		public Drone Drone { get; set; }
		public IList<string> RawLogs { get; set; }
		public IList<MailSent> MailSent { get; set; }
		public IList<MailBounced> MailBounced { get; set; }
		public IList<ClickAction> ClickActions { get; set; }
		public IList<UnsubscribeRequest> UnsubscribeRequests { get; set; }
		public IList<UnclassfiedMailEvent> Unclassified { get; set; }
		public string CurrentCreativeId { get; set; }
	}
}