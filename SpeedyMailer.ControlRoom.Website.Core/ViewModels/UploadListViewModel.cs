using System.Collections.Generic;
using SpeedyMailer.ControlRoom.Website.Core.Models;
using SpeedyMailer.Core.Lists;

namespace SpeedyMailer.ControlRoom.Website.Core.ViewModels
{
    public class UploadListViewModel:UploadListModel
    {
        

        public List<ListDescriptor> Lists { get; set; }

        public bool HasResults { get; set; }

        public ContactsCSVParserResultsViewModel Results { get; set; }

    }

    public class ContactsCSVParserResultsViewModel
    {
        public string NumberOfEmailProcessed { get; set; }

        public string NumberOfFilesProcessed { get; set; }

        public List<string> Filenames { get; set; }
    }
}