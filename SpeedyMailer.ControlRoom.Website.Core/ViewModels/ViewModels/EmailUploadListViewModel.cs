using System.Collections.Generic;

namespace SpeedyMailer.ControlRoom.Website.Core.ViewModels.ViewModels
{
    public class EmailUploadListViewModel
    {
        public string NumberOfEmailProcessed { get; set; }
        public string TimeOfParsing { get; set; }
        public List<string> Filenames { get; set; }
        public string NumberOfFilesProcessed { get; set; }
    }
}