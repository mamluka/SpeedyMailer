using System.Collections.Generic;
using SpeedyMailer.Domain.Model.Lists;
using SpeedyMailer.Master.Web.Core.Models;

namespace SpeedyMailer.Master.Web.Core.ViewModels
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