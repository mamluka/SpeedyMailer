using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Drone.Tests.Drone
{
    public class MockedDroneBuilder : IMockedComponentBuilder<Drone>
    {

        public Drone Build()
        {
            return new Drone();
        }

    }
}