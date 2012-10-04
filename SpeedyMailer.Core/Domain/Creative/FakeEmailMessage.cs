using System.Collections.Generic;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;

namespace SpeedyMailer.Core.Domain.Creative
{
	public class FakeEmailMessage
	{
		public FakeMailAddress From { get; set; }
		public FakeMailAddress Sender { get; set; }
		public IList<FakeMailAddress> ReplyToList { get; set; }
		public IList<FakeMailAddress> To { get; set; }
		public IList<FakeMailAddress> Bcc { get; set; }
		public IList<FakeMailAddress> CC { get; set; }
		public MailPriority Priority { get; set; }
		public DeliveryNotificationOptions DeliveryNotificationOptions { get; set; }
		public string Subject { get; set; }
		public Encoding SubjectEncoding { get; set; }
		public Encoding HeadersEncoding { get; set; }
		public string Body { get; set; }
		public TransferEncoding BodyTransferEncoding { get; set; }
		public bool IsBodyHtml { get; set; }
		public string DroneId { get; set; }
	}
}