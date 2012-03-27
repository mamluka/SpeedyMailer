namespace SpeedyMailer.Bridge.Communication
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