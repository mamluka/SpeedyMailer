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
        public void WeaveDeals_ShouldCallTheUrlCreatorWithTheTheDealsRoute()
        {
            //Arrange
            var dealObject = Fixture.CreateAnonymous<LeadIdentity>();
            var bodySource = EmailSourceFactory.StandardEmail();

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Expect(
                x => x.UrlByRouteWithParameters(Arg<string>.Is.Equal("Deals"), Arg<RouteValueDictionary>.Is.Anything)).
                Repeat.Once().Return("http://www.domain.com/Deals/Object");

            var weaver = new EmailSourceWeaver(urlCreator);
            //Act
            weaver.WeaveDeals(bodySource, dealObject);
            //Assert
            urlCreator.VerifyAllExpectations();
            
        }

        [Test]
        public void WeaveDeals_ShouldCallTheUrlCreatorWithTheJsobObjectInBase64()
        {
            //Arrange
            var dealObject = Fixture.CreateAnonymous<LeadIdentity>();

            var jsonBase64String = UrlCreator.SerializeToBase64(dealObject);


            var bodySource = EmailSourceFactory.StandardEmail();

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Expect(
                x => x.UrlByRouteWithParameters(Arg<string>.Is.Anything, Arg<RouteValueDictionary>.Matches(m => (string) m["JsonObject"] == jsonBase64String))).
                Repeat.Once().Return("http://www.domain.com/Deals/Object");

            var weaver = new EmailSourceWeaver(urlCreator);
            //Act
            weaver.WeaveDeals(bodySource, dealObject);
            //Assert
            urlCreator.VerifyAllExpectations();

        }

        [Test]
        public void WeaveDeals_ShouldReplaceLinksWithDealLinks()
        {
            //Arrange

            var dealObject = Fixture.CreateAnonymous<LeadIdentity>();

            var bodySource = EmailSourceFactory.StandardEmail();
            var urlCreator = MockRepository.GenerateStub<IUrlCreator>();
            urlCreator.Stub(
                x => x.UrlByRouteWithParameters(Arg<string>.Is.Anything, Arg<RouteValueDictionary>.Is.Anything)).Return(
                    "replaced");

            var weaver = new EmailSourceWeaver(urlCreator);
            //Act
            var newBody = weaver.WeaveDeals(bodySource, dealObject);
            var dealList = FindAllDealLinksInAnEmailBody(newBody);
            //Assert
            dealList.Should().OnlyContain(x => x == "replaced");

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
}