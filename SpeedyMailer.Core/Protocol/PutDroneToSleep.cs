namespace SpeedyMailer.Core.Protocol
{
    public class PutDroneToSleep:DroneSideOporationBase
    {
        public PutDroneToSleep()
        {
            DroneSideOporationType = DroneSideOporationType.GoToSleep;
        }
    }
}