using System.Collections.Generic;

namespace SpeedyMailer.Core.DataAccess.Emails
{
    public interface IEmailSourceParser
    {
        List<string> Deals(string emailSource);
    }
}