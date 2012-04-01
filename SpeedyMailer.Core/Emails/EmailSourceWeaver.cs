using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace SpeedyMailer.Core.Emails
{
    public class EmailSourceWeaver : IEmailSourceWeaver
    {

        public string WeaveUnsubscribeTemplate(string bodySource, string template, string unsubscribeLink)
        {
            return bodySource + string.Format(template, unsubscribeLink);
        }

        public string WeaveDeals(string bodySource, string dealLink)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(bodySource);
            List<HtmlNode> dealList =
                doc.DocumentNode.SelectNodes("//a[@href]").ToList();

            foreach (HtmlNode deal in dealList)
            {
                deal.Attributes["href"].Value = dealLink;
            }

            return doc.DocumentNode.InnerHtml;
        }

    }
}