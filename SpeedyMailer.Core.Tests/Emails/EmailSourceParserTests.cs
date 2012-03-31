using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Emails;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailSourceParserTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Deals_ShouldExtractLinksFromTheDocument()
        {
            //Arrange
            string emailSource = EmailSourceFactory.StandardEmail();

            var parser = new EmailSourceParser();
            //Act
            List<string> dealList = parser.Deals(emailSource);
            //Assert
            dealList.Should().Contain("http://www.usocreports.com/switch/aladdin");
        }
    }
}