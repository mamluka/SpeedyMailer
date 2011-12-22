using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using MvcContrib.TestHelper;
using MvcContrib.TestHelper.Fakes;
using NUnit;
using FluentAssertions;
using NUnit.Framework;
using Newtonsoft.Json;
using Rhino.Mocks;
using SpeedyMailer.ControlRoom.Website.Controllers;
using SpeedyMailer.ControlRoom.Website.Tests.Maps;
using SpeedyMailer.Core.Helpers;
using SpeedyMailer.Core.Tests.Emails;
using SpeedyMailer.Tests.Core;


namespace SpeedyMailer.ControlRoom.Website.Tests.Helpers
{
    class UrlCreatorTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [TestFixtureSetUp]
        public void Init()
        {
            var routes = RouteTable.Routes;
           routes.Clear();
            CreateTestRoutes(routes);
            // MvcApplication.RegisterRoutes(CreateTestRoutes());
        }

        [Test]
        public void UrlByRoute_ShouldReturnTheRightUrlForDeals()
        {
            //Arrange
            var urlHelper = CreateFakeUrlHelper();
            var configurationManager = MockRepository.GenerateStub<IConfigurationManager>();
            configurationManager.Stub(x => x.Configurations).Return(new Configurations()
                                                                        {SystemBaseDomainUrl = "http://www.domain.com"});


            var urlCreator = new UrlCreator(urlHelper, configurationManager);
            //Act
            var url = urlCreator.UrlByRouteWithParameters("Deals", new RouteValueDictionary()
                                                                      {
                                                                          {"JsonObject","jsonbase64object"}
                                                                      });

            //Assert
            url.Should().Be("http://www.domain.com/Deals/jsonbase64object");

        }

        private UrlHelper CreateFakeUrlHelper()
        {
            var context = new FakeHttpContext("http://www.domain.com");

            var requestContext = new RequestContext(context, new RouteData());

            var urlHelper = new UrlHelper(requestContext);

            return urlHelper;
        }

        public RouteCollection CreateTestRoutes(RouteCollection routes)
        {
            routes.MapRoute(
                "Deals", // Route name
                "Deals/{JsonObject}", // URL with parameters
                new { controller = "Deals", action = "RedirectToDeal", JsonObject = "{}" } // Parameter defaults
            );
            return routes;
        }
    }

    public class UrlCreator : IUrlCreator
    {
        private readonly UrlHelper urlHelper;
        private readonly IConfigurationManager configurationManager;

        public UrlCreator(UrlHelper urlHelper, IConfigurationManager configurationManager)
        {
            this.urlHelper = urlHelper;
            this.configurationManager = configurationManager;
        }

        public string UrlByRoute(string routeName)
        {
            return configurationManager.Configurations.SystemBaseDomainUrl +  urlHelper.RouteUrl(routeName,new RouteValueDictionary());
        }

        public string UrlByRouteWithParameters(string routeName,RouteValueDictionary dictionary)
        {
            return configurationManager.Configurations.SystemBaseDomainUrl + urlHelper.RouteUrl(routeName, dictionary);
        }

        public string SerializeToBase64(dynamic whatToEncode)
        {
            var jsonObject = JsonConvert.SerializeObject(whatToEncode);

            var toEncodeAsBytes = Encoding.UTF8.GetBytes(jsonObject);

            var returnValue = Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;
        }
    }
}
