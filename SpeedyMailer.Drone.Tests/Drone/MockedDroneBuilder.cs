using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Drone.Tests.Drone
{
    public class MockedDroneBuilder : IMockedComponentBuilder<Drone>
    {
        #region IMockedComponentBuilder<Drone> Members

        public Drone Build()
        {
            return new Drone();
        }

        #endregion
    }
}