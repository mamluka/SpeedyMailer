using System.Collections.Generic;
using SpeedyMailer.Core.MailDrones;

namespace SpeedyMailer.Core.Emails
{
    public class EmailFragment
    {
        public string Body { get; set; }
        public List<ExtendedRecipient> ExtendedRecipients { get; set; }
        public string Subject { get; set; }
        public string Id { get; set; }

        public string UnsubscribeTemplate { get; set; }
        public int CreateDate { get; set; }
        public bool Locked { get; set; }

        public FragmentStatus Status { get; set; }
        public MailDrone CompletedBy { get; set; }


    }

    public enum FragmentStatus
    {
        Pending,
        Completed
    }

    public class ExtendedRecipient
    {
        public string Address { get; set; }
        public string Name { get; set; }
        public string DealUrl { get; set; }
        public string UnsubscribeUrl { get; set; }
    }
}