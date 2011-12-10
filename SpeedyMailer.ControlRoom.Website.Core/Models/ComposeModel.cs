using System.Collections.Generic;
using SpeedyMailer.ControlRoom.Website.Core.ComponentViewModel;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;

namespace SpeedyMailer.ControlRoom.Website.Core.Models
{
    public class ComposeModel
    {
        public List<string> ToLists { get; set; }
        public string Body { get; set; }

        

    }
}