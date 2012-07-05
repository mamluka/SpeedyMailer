using System.Collections.Generic;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Core.Domain.Creative
{
    public class CreativeFragment
    {
    	public string Id { get; set; }
        public Creative Creative { get; set; }
        public string UnsubscribeTemplate { get; set; }
        public List<Contact> Recipients { get; set; }
    }
}