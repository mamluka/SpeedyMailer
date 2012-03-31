using System.Collections.Generic;
using SpeedyMailer.Bridge.Model.Drones;

namespace SpeedyMailer.Bridge.Model.Fragments
{
    public class EmailFragment
    {
        public string Body { get; set; }
        public List<ExtendedRecipient> ExtendedRecipients { get; set; }
        public string Subject { get; set; }
        public string Id { get; set; }

        public string UnsubscribeTemplate { get; set; }
        public long CreateDate { get; set; }
        public bool Locked { get; set; }

        public FragmentStatus Status { get; set; }
        public MailDrone CompletedBy { get; set; }

        public string MailId { get; set; }
    }
}