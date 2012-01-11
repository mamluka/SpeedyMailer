using System.Collections.Generic;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Core.Protocol
{
    public class FragmentResponse
    {
        public EmailFragment EmailFragment { get; set; }
        public List<DroneSideOporationBase> DroneSideOporations { get; set; }

      
    }
}