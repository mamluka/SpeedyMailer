namespace SpeedyMailer.Core.Settings
{
    public interface ICreativeFragmentSettings
    {
        [Default(1000)]
        int RecipientsPerFragment { get; set; }
    }
}