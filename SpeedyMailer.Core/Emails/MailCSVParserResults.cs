using System;
using System.Collections.Generic;

namespace SpeedyMailer.Core.Emails
{
    public class MailCSVParserResults
    {
        public int NumberOfEmailProcessed { get; set; }
        public DateTime TimeOfParsing { get; set; }
        public List<string> Filenames { get; set; }

        public int NumberOfFilesProcessed { get; set; }
    }
}