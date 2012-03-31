using System.Collections.Generic;
using SpeedyMailer.Master.Web.Core.ComponentViewModel;
using SpeedyMailer.Master.Web.Core.Models;

namespace SpeedyMailer.Master.Web.Core.ViewModels
{
    public class ComposeViewModel : ComposeModel
    {
        public List<ListDescriptorViewModel> AvailableLists { get; set; }
        public ComposeResultsViewModel Results { get; set; }
    }
}