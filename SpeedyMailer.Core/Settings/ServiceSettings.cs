namespace SpeedyMailer.Core.Settings
{
    public class ServiceSettings
    {
        [Default("http://localhost:12345")]
        public virtual string ServiceBaseUrl { get; set; }
    }
}
