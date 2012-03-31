using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;

namespace SpeedyMailer.Core.Helpers
{
    public class UrlCreator : IUrlCreator
    {
        private readonly IConfigurationManager configurationManager;
        private readonly UrlHelper urlHelper;

        public UrlCreator(UrlHelper urlHelper, IConfigurationManager configurationManager)
        {
            this.urlHelper = urlHelper;
            this.configurationManager = configurationManager;
        }

        #region IUrlCreator Members

        public string UrlByRoute(string routeName)
        {
            return configurationManager.ControlRoomConfigurations.DomainUrl +
                   urlHelper.RouteUrl(routeName, new RouteValueDictionary());
        }

        public string UrlByRouteWithParameters(string routeName, RouteValueDictionary dictionary)
        {
            return configurationManager.ControlRoomConfigurations.DomainUrl + urlHelper.RouteUrl(routeName, dictionary);
        }

        public string UrlByRouteWithJsonObject(string routeName, dynamic jsonObject)
        {
            return configurationManager.ControlRoomConfigurations.DomainUrl +
                   urlHelper.RouteUrl(routeName, new RouteValueDictionary
                                                     {
                                                         {"JsonObject", SerializeToBase64(jsonObject)}
                                                     });
        }

        #endregion

        public static string SerializeToBase64(dynamic whatToEncode)
        {
            dynamic jsonObject = JsonConvert.SerializeObject(whatToEncode);

            dynamic toEncodeAsBytes = Encoding.UTF8.GetBytes(jsonObject);

            dynamic returnValue = Convert.ToBase64String(toEncodeAsBytes);


            return returnValue;
        }
    }
}