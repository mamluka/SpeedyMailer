using System.Collections.Generic;
using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Bridge.Communication
{
    public class FragmentResponse
    {
        public EmailFragment EmailFragment { get; set; }
        public List<DroneSideOporationBase> DroneSideOporations { get; set; }
    }
}