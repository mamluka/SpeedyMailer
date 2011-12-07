using System.Collections.Generic;
using SpeedyMailer.ControlRoom.Website.Core.ComponentViewModel;

namespace SpeedyMailer.ControlRoom.Website.Core.ViewModels
{
    public class ComposeViewModel
    {
        public List<ListDescriptorViewModel> Lists { get; set; }
        public string EmailBody { get; set; }
    }
}