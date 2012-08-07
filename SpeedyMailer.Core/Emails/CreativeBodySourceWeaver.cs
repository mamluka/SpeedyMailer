using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace SpeedyMailer.Core.Emails
{
    public class CreativeBodySourceWeaver : ICreativeBodySourceWeaver
    {

        public string WeaveUnsubscribeTemplate(string bodySource, string template, string unsubscribeLink)
        {
            return bodySource + string.Format(template, unsubscribeLink);
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