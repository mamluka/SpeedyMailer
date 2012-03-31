using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Emails;


namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailSourceParserTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Deals_ShouldExtractLinksFromTheDocument()
        {
            //Arrange
            var emailSource = EmailSourceFactory.StandardEmail();

            var parser = new EmailSourceParser();
            //Act
            var dealList = parser.Deals(emailSource);
            //Assert
            dealList.Should().Contain("http://www.usocreports.com/switch/aladdin");

        }

       
    }
}