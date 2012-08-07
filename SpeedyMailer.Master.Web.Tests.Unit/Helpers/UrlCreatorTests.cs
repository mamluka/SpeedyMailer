using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentAssertions;
using MvcContrib.TestHelper.Fakes;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using SpeedyMailer.Core.Domain.Emails;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Utilities.Domain.Email;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Master.Web.Tests.Unit.Helpers
{
    internal class UrlCreatorTests : AutoMapperAndFixtureBase
    {
        [TestFixtureSetUp]
        public void Init()
        {
            RouteCollection routes = RouteTable.Routes;
            routes.Clear();
            CreateTestRoutes(routes);
        }

        [Test]
        public void UrlByRoute_ShouldReturnTheRightUrlForDeals()
        {
            var urlHelper = CreateFakeUrlHelper();
            var settings = MockRepository.GenerateStub<ServiceSettings>();
            settings.ServiceBaseUrl = "http://www.domain.com";

            var urlCreator = new UrlCreator(urlHelper, settings);

            var url = urlCreator.UrlByRouteWithParameters("Deals", new RouteValueDictionary
                                                                          {
                                                                              {"JsonObject", "jsonbase64object"}
                                                                          });

            url.Should().Be("http://www.domain.com/Deals/jsonbase64object");
        }

        [Test]
        public void UrlByRouteWithJsonObject_ShouldReturnAUrlWithBase64JsonObject()
        {
            var jsonObject = Fixture.CreateAnonymous<LeadIdentity>();

            var jsonString = EmulateDynamicEncoding(jsonObject);

            var urlHelper = CreateFakeUrlHelper();
            var settings = MockRepository.GenerateStub<ServiceSettings>();
            settings.ServiceBaseUrl = "http://www.domain.com";


            var urlCreator = new UrlCreator(urlHelper, settings);
            var url = urlCreator.UrlByRouteWithJsonObject("Deals", jsonObject);

            url.Should().Be("http://www.domain.com/Deals/" + HttpUtility.UrlEncode(jsonString));
        }

        private string EmulateDynamicEncoding(dynamic jsonObject)
        {
            return UrlCreator.SerializeToBase64(jsonObject);
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
                new {controller = "Deals", action = "RedirectToDeal", JsonObject = "{}"} // Parameter defaults
                );
            return routes;
        }
    }
}