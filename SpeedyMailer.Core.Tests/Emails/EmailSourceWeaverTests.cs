using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Emails;


namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailSourceWeaverTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        [Ignore]
        public void WeaveDeals_ShouldReplaceTheLinksInTheEmailWithTheWeavenLinks()
        {
            //Arrange
            var dealObject = Fixture.CreateAnonymous<DealURLJsonObject>();
            var bodySource = EmailSourceFactory.StandardEmail();

            var weaver = new EmailSourceWeaver();
            //Act
            var newSource = weaver.WeaveDeals(bodySource, dealObject);
            //Assert
         //   newSource.Should().Contain("http://")


        }
    }

    public class DealURLJsonObject
    {
        public string Email { get; set; }
        public string Contact { get; set; }
    }

    public class EmailSourceWeaver
    {
        public string WeaveDeals(string bodySource, DealURLJsonObject dealObject)
        {
            
        }
    }
}