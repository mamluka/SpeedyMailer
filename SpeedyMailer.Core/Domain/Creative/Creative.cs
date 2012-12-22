using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Creative
{
    public class Creative
    {
        public string Id { get; set; }
        public string HtmlBody { get; set; }
	    public string TextBody { get; set; }
	    public string Subject { get; set; }
	    public string DealUrl { get; set; }
	    public List<string> Lists { get; set; }
	    public string UnsubscribeTemplateId { get; set; }
	    public string FromName { get; set; }
	    public string FromAddressDomainPrefix { get; set; }
    }
}