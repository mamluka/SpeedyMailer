using System.Collections.Generic;

namespace SpeedyMailer.Core.Emails
{
    public class Email
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public List<string> ToLists { get; set; }
    }
}