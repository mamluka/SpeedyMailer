using System;
using System.Collections.Generic;

namespace SpeedyMailer.Utilties.Domain.Contacts
{
    public class ContactCSVParserResults
    {
        public int NumberOfContactsProcessed { get; set; }
        public DateTime TimeOfParsing { get; set; }
        public List<string> Filenames { get; set; }

        public int NumberOfFilesProcessed { get; set; }
    }
}