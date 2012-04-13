using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SpeedyMailer.Core.Commands;

namespace SpeedyMailer.Master.Web.Core.Commands
{
    public class UploadListCommand : Command<UploadListCommandResult>
     
    {
        public Stream Source { get; set; }
        public string ListId { get; set; }

        public override UploadListCommandResult Execute()
        {
            return null;
        }
    }

    public class UploadListCommandResult
    {
        public string Filename { get; set; }
        public long NumberOfContacts { get; set; }
    }
}
