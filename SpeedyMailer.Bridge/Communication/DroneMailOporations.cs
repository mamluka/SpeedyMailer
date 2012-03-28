using System;

namespace SpeedyMailer.Bridge.Communication
{
    public class DroneMailOporations : IDroneMailOporations
    {

        public Action StopCurrentJob { get; set; }

        public void Preform(DroneSideOporationBase poolSideOporation)
        {
            switch (poolSideOporation.DroneSideOporationType)
            {
                case DroneSideOporationType.GoToSleep:
                    GoToSleep(poolSideOporation as GoToSleepOporation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GoToSleep(GoToSleepOporation goToSleepOporation)
        {
            StopCurrentJob.Invoke();
        }
    }
}