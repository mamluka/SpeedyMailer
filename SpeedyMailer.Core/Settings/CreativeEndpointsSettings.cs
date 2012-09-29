namespace SpeedyMailer.Core.Settings
{
	public class CreativeEndpointsSettings
	{
		[Default("deals")]
		public virtual string Deal { get; set; }
		[Default("lists/unsubscribe")]
		public virtual string Unsubscribe { get; set; }
	}
}