using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;

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