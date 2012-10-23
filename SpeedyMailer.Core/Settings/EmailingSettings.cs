namespace SpeedyMailer.Core.Settings
{
	public class EmailingSettings
	{
		public virtual string WritingEmailsToDiskPath { get; set; }

		[Default("localhost")]
		public virtual string SmtpHost { get; set; }

		[Default("localhost")]
		public virtual string MailingDomain { get; set; }
	}
}