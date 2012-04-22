namespace SpeedyMailer.Core.Settings
{
    public interface IServiceSettings
    {
        [Default("http://localhost:12345")]
        string ServiceBaseUrl { get; set; }
    }
}
