using System.Collections.Generic;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Master;

namespace SpeedyMailer.Core.Domain.Creative
{
    public class CreativeFragment
    {
    	public string Id { get; set; }
        public string UnsubscribeTemplate { get; set; }
        public List<Contact> Recipients { get; set; }
    	public string Body { get; set; }
    	public string Subject { get; set; }
    	public Service Service { get; set; }
    	public string CreativeId { get; set; }
    }
}