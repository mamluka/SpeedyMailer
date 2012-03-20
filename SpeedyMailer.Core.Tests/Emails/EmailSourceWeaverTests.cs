using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Emails;


namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class EmailSourceWeaverTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void WeaveDeals_ShouldShouldReplaceTheLinksWithDealLinksWithStrings()
        {
            //Arrange
            var bodySource = EmailSourceFactory.StandardEmail();
            var link = "link";
            var builder = new MockedEmailSourceWeaverBuilder();
            var weaver = builder.Build();
            //Act

            var newBody = weaver.WeaveDeals(bodySource, link);
            var dealList = FindAllDealLinksInAnEmailBody(newBody);
            //Assert

            //Assert
            dealList.Should().OnlyContain(x => x == "link");
        }

        [Test]
        public void WeaveUnsubscribeTemplate_ShouldAddTheTemplateToTheEndOfTheEmail()
        {
            //Arrange
            var bodySource = EmailSourceFactory.StandardEmail();
            var template = "This template is used in the test and here is the link {0}";
            var unsubscribeLink = "link";

            var combined = String.Format(template, unsubscribeLink);

            var builder = new MockedEmailSourceWeaverBuilder();
            var weaver = builder.Build();
            //Act
            var newBody = weaver.WeaveUnsubscribeTemplate(bodySource, template, unsubscribeLink);
            //Assert
            newBody.Should().EndWithEquivalent(combined);


        }

        private List<string> FindAllDealLinksInAnEmailBody(string body)
        {
            var parser = new EmailSourceParser();
            //Act
            return parser.Deals(body);
        }

       

        static public string EncodeTo64(string toEncode)
        {

            var toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);

            var returnValue = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;


        }
    }

    public class MockedEmailSourceWeaverBuilder:IMockedComponentBuilder<EmailSourceWeaver>
    {


        
        public EmailSourceWeaver Build()
        {
            return new EmailSourceWeaver();
        }
    }
}