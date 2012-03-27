using System.Collections.Generic;

namespace SpeedyMailer.Domain.Model.Emails
{
    public class Email
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public List<string> ToLists { get; set; }
        public string Subject { get; set; }
        public List<string> Deals { get; set; }
    }
}