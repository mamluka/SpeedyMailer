namespace SpeedyMailer.Bridge.Communication
{
    public class PutDroneToSleep : DroneSideOporationBase
    {
        public PutDroneToSleep()
        {
            DroneSideOporationType = DroneSideOporationType.GoToSleep;
        }
    }
}