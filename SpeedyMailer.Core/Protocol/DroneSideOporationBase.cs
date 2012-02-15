namespace SpeedyMailer.Core.Protocol
{
    public class DroneSideOporationBase
    {
        public DroneSideOporationType DroneSideOporationType;
    }

    public class GoToSleepOporation : DroneSideOporationBase
    {
        public GoToSleepOporation()
        {
            DroneSideOporationType = DroneSideOporationType.GoToSleep;
        }
    }
}