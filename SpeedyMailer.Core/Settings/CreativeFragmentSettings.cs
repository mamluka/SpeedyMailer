namespace SpeedyMailer.Core.Settings
{
    public class CreativeFragmentSettings
    {
        [Default(200)]
        public virtual int RecipientsPerFragment { get; set; }

		[Default(30)]
	    public virtual int DefaultInterval { get; set; }

		[Default("$default$")]
	    public virtual string DefaultGroup { get; set; }
    }
}