using System.IO;
using System.Linq;
using System.Web.Routing;
using HtmlAgilityPack;
using SpeedyMailer.Core.Helpers;

namespace SpeedyMailer.Core.Emails
{
    public class EmailSourceWeaver : IEmailSourceWeaver
    {
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