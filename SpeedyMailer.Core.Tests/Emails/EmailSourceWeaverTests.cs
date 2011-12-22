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
            var dealObject = Fixture.CreateAnonymous<DealURLJsonObject>();
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
            var dealObject = Fixture.CreateAnonymous<DealURLJsonObject>();

            var jsonBase64String = SerializeToBase64(dealObject);


            var bodySource = EmailSourceFactory.StandardEmail();

            var urlCreator = MockRepository.GenerateMock<IUrlCreator>();
            urlCreator.Stub(x => x.SerializeToBase64(Arg<string>.Is.Anything)).Return(jsonBase64String);
            urlCreator.Expect(
                x => x.UrlByRouteWithParameters(Arg<string>.Is.Anything, Arg<RouteValueDictionary>.Matches(m => (string) m["JsonObject"] == jsonBase64String))).
                Repeat.Once().Return("http://www.domain.com/Deals/Object");

            var weaver = new EmailSourceWeaver(urlCreator);
            //Act
            weaver.WeaveDeals(bodySource, dealObject);
            //Assert
            urlCreator.VerifyAllExpectations();

        }

        private string SerializeToBase64(DealURLJsonObject dealObject)
        {
            var jsonObject = JsonConvert.SerializeObject(dealObject);

            return EncodeTo64(jsonObject);
        }

        static public string EncodeTo64(string toEncode)
        {

            var toEncodeAsBytes = Encoding.UTF8.GetBytes(toEncode);

            var returnValue = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;


        }
    }

    public interface IConfigurationManager
    {
        Configurations Configurations { get; }
    }

    public class Configurations
    {
        public string SystemBaseDomainUrl { get; set; }
    }

    public class DealURLJsonObject
    {
        public string Email { get; set; }
        public string Contact { get; set; }
    }

    public class EmailSourceWeaver
    {
        private readonly IUrlCreator urlCreator;

        public EmailSourceWeaver(IUrlCreator urlCreator)
        {
            this.urlCreator = urlCreator;
        }

        public string WeaveDeals(string bodySource, DealURLJsonObject dealObject)
        {
            var jsonBase64String = urlCreator.SerializeToBase64(dealObject);
            var url = urlCreator.UrlByRouteWithParameters("Deals", new RouteValueDictionary()
                                                                       {
                                                                           {"JsonObject",jsonBase64String}
                                                                       });
            return bodySource;
        }
    }
}