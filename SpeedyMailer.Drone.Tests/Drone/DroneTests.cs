using NUnit.Framework;
using SpeedyMailer.Drone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using Quartz;

namespace SpeedyMailer.Drone.Tests.Drone
{
    [TestFixture]
    public class DroneTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Start_ShouldStartTheDroneJob()
        {
            //Arrange
            var schedular = MockRepository.GenerateStub<IScheduler>();

            var builder = new MockedDroneBuilder();
            var drone = builder.Build();
            //Act

            //Assert

        }

    }

    public class MockedDroneBuilder:IMockedComponentBuilder<Drone>
    {
        public Drone Build()
        {
            return new Drone();
        }
    }

    public class Drone
    {
         
    }
}
