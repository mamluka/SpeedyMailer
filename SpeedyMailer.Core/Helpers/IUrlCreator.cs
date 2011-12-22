using System.Web.Routing;

namespace SpeedyMailer.Core.Helpers
{
    public interface IUrlCreator
    {
        string UrlByRoute(string routeName);
        string UrlByRouteWithParameters(string routeName,RouteValueDictionary dictionary);
        string SerializeToBase64(dynamic whatToEncode);
    }
}