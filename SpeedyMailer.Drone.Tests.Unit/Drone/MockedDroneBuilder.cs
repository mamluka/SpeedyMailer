using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Drone
{
    public class MockedDroneBuilder : IMockedComponentBuilder<Drone>
    {

        public Drone Build()
        {
            return new Drone();
        }

    }
}