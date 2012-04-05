using System.Web.Routing;

namespace SpeedyMailer.Utilties.Domain.Email
{
    public interface IUrlCreator
    {
        string UrlByRoute(string routeName);
        string UrlByRouteWithParameters(string routeName, RouteValueDictionary dictionary);
        string UrlByRouteWithJsonObject(string routeName, dynamic jsonObject);
    }
}