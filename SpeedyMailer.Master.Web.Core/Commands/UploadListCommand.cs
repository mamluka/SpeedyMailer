using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Master.Web.Core.Commands
{
    public class UploadListCommand
    {
        public Stream Source { get; set; }
        public long ListId { get; set; }
    }
}
