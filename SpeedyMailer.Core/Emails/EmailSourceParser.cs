using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace SpeedyMailer.Core.Emails
{
    public class EmailSourceParser : IEmailSourceParser
    {
        public List<string> Deals(string emailSource)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(emailSource);
            var dealList = doc.DocumentNode.SelectNodes("//a[@href]").Select(link => link.GetAttributeValue("href", "")).ToList();
            return dealList.Distinct().ToList();
        }
    }
}