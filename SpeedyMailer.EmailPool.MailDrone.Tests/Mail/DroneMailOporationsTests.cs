using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.MailDrone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;

namespace SpeedyMailer.MailDrone.Tests.Mail
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
