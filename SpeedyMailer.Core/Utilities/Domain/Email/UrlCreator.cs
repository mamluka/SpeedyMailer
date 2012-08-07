using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Core.Utilities.Domain.Email
{
	public interface IUrlCreator
	{
		string UrlByRoute(string routeName);
		string UrlByRouteWithParameters(string routeName, RouteValueDictionary dictionary);
		string UrlByRouteWithJsonObject(string routeName, dynamic jsonObject);
	}

	public class UrlCreator : IUrlCreator
	{
		private readonly UrlHelper _urlHelper;
		private readonly ServiceSettings _serviceSettings;

		public UrlCreator(UrlHelper urlHelper, ServiceSettings serviceSettings)
		{
			_urlHelper = urlHelper;
			_serviceSettings = serviceSettings;
		}


		public string UrlByRoute(string routeName)
		{
			return _serviceSettings.ServiceBaseUrl +
				   _urlHelper.RouteUrl(routeName, new RouteValueDictionary());
		}

		public string UrlByRouteWithParameters(string routeName, RouteValueDictionary dictionary)
		{
			return _serviceSettings.ServiceBaseUrl + _urlHelper.RouteUrl(routeName, dictionary);
		}

		public string UrlByRouteWithJsonObject(string routeName, dynamic jsonObject)
		{
			return _serviceSettings.ServiceBaseUrl +
				   _urlHelper.RouteUrl(routeName, new RouteValueDictionary
                                                     {
                                                         {"JsonObject", SerializeToBase64(jsonObject)}
                                                     });
		}


		public static string SerializeToBase64(dynamic whatToEncode)
		{
			dynamic jsonObject = JsonConvert.SerializeObject(whatToEncode);

			dynamic toEncodeAsBytes = Encoding.UTF8.GetBytes(jsonObject);

			dynamic returnValue = Convert.ToBase64String(toEncodeAsBytes);


			return returnValue;
		}
	}
}