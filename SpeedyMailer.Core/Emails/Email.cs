using System.Collections.Generic;

namespace SpeedyMailer.Core.Emails
{
    public class Email
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public List<string> MemberOf { get; set; }

        public Email()
        {
            MemberOf = new List<string>();
        }
    }
}