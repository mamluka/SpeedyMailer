namespace SpeedyMailer.Core.Settings
{
    public class CreativeFragmentSettings
    {
        [Default(1000)]
        public virtual int RecipientsPerFragment { get; set; }

		[Default(1)]
	    public virtual int DefaultInterval { get; set; }
    }
}