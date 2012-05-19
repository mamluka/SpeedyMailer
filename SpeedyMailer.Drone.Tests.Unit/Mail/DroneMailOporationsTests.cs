using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Mail
{
    [TestFixture]
    public class DroneMailOporationsTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Preform_ShouldActiveTheStopCorrentJobActionWhenGettingASleepOporation()
        {
            //Arrange
            bool checkBool = false;
            var oporation = new GoToSleepOporation();
            //Act
            var mailOporations = new DroneMailOporations();

            mailOporations.StopCurrentJob = () => { checkBool = true; };


            //Assert
            mailOporations.Preform(oporation);
            checkBool.Should().BeTrue();
        }
    }
}