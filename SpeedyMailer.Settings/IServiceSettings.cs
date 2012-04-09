using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpeedyMailer.Settings
{
    public interface IServiceSettings
    {
        [Default("http://localhost:12345")]
        string ServiceBaseUrl { get; set; }
    }
}
