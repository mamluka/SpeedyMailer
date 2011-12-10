using System.Collections.Generic;
using SpeedyMailer.ControlRoom.Website.Core.ComponentViewModel;
using SpeedyMailer.ControlRoom.Website.Core.Models;

namespace SpeedyMailer.ControlRoom.Website.Core.ViewModels
{
    public class ComposeViewModel:ComposeModel
    {
        public List<ListDescriptorViewModel> AvalibaleLists { get; set; }
        public ComposeResultsViewModel Results { get; set; }
    }
}