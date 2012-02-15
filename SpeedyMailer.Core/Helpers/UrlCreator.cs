using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;

namespace SpeedyMailer.Core.Helpers
{
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
            return configurationManager.ControlRoomConfigurations.DomainUrl +  urlHelper.RouteUrl(routeName,new RouteValueDictionary());
        }

        public string UrlByRouteWithParameters(string routeName,RouteValueDictionary dictionary)
        {
            return configurationManager.ControlRoomConfigurations.DomainUrl + urlHelper.RouteUrl(routeName, dictionary);
        }

        public string UrlByRouteWithJsonObject(string routeName, dynamic jsonObject)
        {
            return configurationManager.ControlRoomConfigurations.DomainUrl + urlHelper.RouteUrl(routeName, new RouteValueDictionary()
                                                                                                                {
                                                                                                                     {"JsonObject",SerializeToBase64(jsonObject)} 
                                                                                                                });
        }

        public static string SerializeToBase64(dynamic whatToEncode)
        {
            var jsonObject = JsonConvert.SerializeObject(whatToEncode);

            var toEncodeAsBytes = Encoding.UTF8.GetBytes(jsonObject);

            var returnValue = Convert.ToBase64String(toEncodeAsBytes);



            return returnValue;
        }
    }
}