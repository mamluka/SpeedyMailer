using System.Collections.Generic;

namespace SpeedyMailer.Core.Domain.Creative
{
    public class Creative
    {
        public string Id { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public List<string> Lists { get; set; }
    }
}