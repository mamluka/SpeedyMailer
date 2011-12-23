using System.IO;
using System.Linq;
using System.Web.Routing;
using HtmlAgilityPack;
using SpeedyMailer.Core.Helpers;

namespace SpeedyMailer.Core.Emails
{
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
                                                                           {"JsonObject", jsonBase64String}
                                                                       });


            var doc = new HtmlDocument();
            doc.LoadHtml(bodySource);
            var dealList =
                doc.DocumentNode.SelectNodes("//a[@href]").ToList();

            foreach (var deal in dealList)
            {
                deal.Attributes["href"].Value = url;
            }

            return doc.DocumentNode.InnerHtml;
        }
    }
}