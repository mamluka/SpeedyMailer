using System.Collections.Generic;

namespace SpeedyMailer.Core.Emails
{
    public class EmailFragment
    {
        public string Body { get; set; }
        public List<string> Recipients { get; set; }
        public string Subject { get; set; }
        public string Id { get; set; }

        public bool Locked { get; set; }
    }
}