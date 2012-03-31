using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;

namespace SpeedyMailer.Core.DataAccess.Emails
{
    public class EmailSourceParser : IEmailSourceParser
    {
        #region IEmailSourceParser Members

        public List<string> Deals(string emailSource)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(emailSource);
            HtmlNodeCollection extractedDealList = doc.DocumentNode.SelectNodes("//a[@href]");
            if (extractedDealList != null)
            {
                return
                    extractedDealList.Select(link => link.GetAttributeValue("href", "")).Distinct().
                        ToList();
            }
            return new List<string>();
        }

        #endregion
    }
}