using System.IO;
using System.Linq;
using System.Web.Routing;
using HtmlAgilityPack;
using SpeedyMailer.Core.Helpers;

namespace SpeedyMailer.Core.Emails
{
    public class EmailSourceWeaver : IEmailSourceWeaver
    {
        private readonly IUrlCreator urlCreator;

        public EmailSourceWeaver(IUrlCreator urlCreator)
        {
            this.urlCreator = urlCreator;
        }

        public string WeaveDeals(string bodySource, LeadIdentity dealObject)
        {
            var jsonBase64String = UrlCreator.SerializeToBase64(dealObject);
            var url = urlCreator.UrlByRouteWithParameters("Deals", new RouteValueDictionary()
                                                                       {
                                                                           {"JsonObject", jsonBase64String}
                                                                       });

            return WeaveDeals(bodySource, url);



        }


        public string WeaveUnsubscribeTemplate(string bodySource, string template, string unsubscribeLink)
        {
            return bodySource + string.Format(template,unsubscribeLink);
        }

        public string WeaveDeals(string bodySource, string dealLink)
        {
             var doc = new HtmlDocument();
            doc.LoadHtml(bodySource);
            var dealList =
                doc.DocumentNode.SelectNodes("//a[@href]").ToList();

            foreach (var deal in dealList)
            {
                deal.Attributes["href"].Value = dealLink;
            }

            return doc.DocumentNode.InnerHtml;
        }
    }
}