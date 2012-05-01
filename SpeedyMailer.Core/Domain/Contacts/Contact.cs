using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Contacts
{
    public class Contact
    {
        public Contact()
        {
            MemberOf = new List<string>();
        }

        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public List<string> MemberOf { get; set; }
    }
}