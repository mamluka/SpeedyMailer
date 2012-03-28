using System.Collections.Generic;

namespace SpeedyMailer.Domain.Model.Contacts
{
    public class Contact
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public List<string> MemberOf { get; set; }

        public Contact()
        {
            MemberOf = new List<string>();
        }
    }
}