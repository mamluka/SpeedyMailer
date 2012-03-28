using System.Collections.Generic;

namespace SpeedyMailer.Domain.DataAccess.Emails
{
    public interface IEmailSourceParser
    {
        List<string> Deals(string emailSource);
    }
}