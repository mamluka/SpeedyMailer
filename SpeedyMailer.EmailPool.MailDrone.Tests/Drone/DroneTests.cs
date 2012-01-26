using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.MailDrone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using Quartz;
namespace SpeedyMailer.MailDrone.Tests.Drone
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
