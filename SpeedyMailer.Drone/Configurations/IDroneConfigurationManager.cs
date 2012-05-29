namespace SpeedyMailer.Drone.Configurations
{
    public interface IDroneConfigurationManager
    {
        string BasePoolUrl { get; set; }
        PoolOporationsUrls PoolOporationsUrls { get; set; }
    }
}