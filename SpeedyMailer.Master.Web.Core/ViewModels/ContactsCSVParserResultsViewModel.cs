using System.Collections.Generic;

namespace SpeedyMailer.Master.Web.Core.ViewModels
{
    public class ContactsCSVParserResultsViewModel
    {
        public string NumberOfEmailProcessed { get; set; }

        public string NumberOfFilesProcessed { get; set; }

        public List<string> Filenames { get; set; }
    }
}