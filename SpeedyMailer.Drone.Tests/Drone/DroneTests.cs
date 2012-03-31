using NUnit.Framework;
using Quartz;
using Rhino.Mocks;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Drone.Tests.Drone
{
    [TestFixture]
    public class DroneTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Start_ShouldStartTheDroneJob()
        {
            //Arrange
            var schedular = MockRepository.GenerateStub<IScheduler>();

            var builder = new MockedDroneBuilder();
            Drone drone = builder.Build();
            //Act

            //Assert
        }
    }
}