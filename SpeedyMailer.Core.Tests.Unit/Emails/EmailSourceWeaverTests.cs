using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.DataAccess.Emails;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Tests.Core.Emails;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.Tests.Unit.Emails
{
    [TestFixture]
    public class EmailSourceWeaverTests : AutoMapperAndFixtureBase
    {
        private List<string> FindAllDealLinksInAnEmailBody(string body)
        {
            var parser = new EmailSourceParser();
            //Act
            return parser.Deals(body);
        }


        public static string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);

            string returnValue = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;
        }

        [Test]
        public void WeaveDeals_ShouldShouldReplaceTheLinksWithDealLinksWithStrings()
        {
            //Arrange
            string bodySource = EmailSourceFactory.StandardEmail();
            string link = "link";
            var builder = new MockedEmailSourceWeaverBuilder();
            EmailSourceWeaver weaver = builder.Build();
            //Act

            string newBody = weaver.WeaveDeals(bodySource, link);
            List<string> dealList = FindAllDealLinksInAnEmailBody(newBody);
            //Assert

            //Assert
            dealList.Should().OnlyContain(x => x == "link");
        }

        [Test]
        public void WeaveUnsubscribeTemplate_ShouldAddTheTemplateToTheEndOfTheEmail()
        {
            //Arrange
            string bodySource = EmailSourceFactory.StandardEmail();
            string template = "This template is used in the test and here is the link {0}";
            string unsubscribeLink = "link";

            string combined = String.Format(template, unsubscribeLink);

            var builder = new MockedEmailSourceWeaverBuilder();
            EmailSourceWeaver weaver = builder.Build();
            //Act
            string newBody = weaver.WeaveUnsubscribeTemplate(bodySource, template, unsubscribeLink);
            //Assert
            newBody.Should().EndWithEquivalent(combined);
        }
    }
}