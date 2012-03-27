using NUnit.Framework;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.Drone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using FluentAssertions;

namespace SpeedyMailer.Drone.Tests.Mail
{
    [TestFixture]
    public class DroneMailOporationsTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Preform_ShouldActiveTheStopCorrentJobActionWhenGettingASleepOporation()
        {
            //Arrange
            var checkBool = false;
            var oporation = new GoToSleepOporation();
            //Act
            var mailOporations = new DroneMailOporations();

            mailOporations.StopCurrentJob = () =>
                                                {
                                                    checkBool = true;
                                                };
            
            
            //Assert
            mailOporations.Preform(oporation);
            checkBool.Should().BeTrue();

        }
    }
}
