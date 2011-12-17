using System.Collections.Generic;

namespace SpeedyMailer.Core.Emails
{
    public interface IEmailSourceParser
    {
        List<string> Deals(string emailSource);
    }
}