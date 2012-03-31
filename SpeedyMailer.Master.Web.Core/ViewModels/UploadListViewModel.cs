using System.Collections.Generic;
using SpeedyMailer.Domain.Lists;
using SpeedyMailer.Master.Web.Core.Models;

namespace SpeedyMailer.Master.Web.Core.ViewModels
{
    public class UploadListViewModel : UploadListModel
    {
        public List<ListDescriptor> Lists { get; set; }

        public bool HasResults { get; set; }

        public ContactsCSVParserResultsViewModel Results { get; set; }
    }
}