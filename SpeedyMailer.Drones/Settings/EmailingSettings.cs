using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Drones.Settings
{
	public class EmailingSettings
	{
		public virtual string WritingEmailsToDiskPath { get; set; }

		[Default("localhost")]
		public virtual string SmtpHost { get; set; }
	}
}