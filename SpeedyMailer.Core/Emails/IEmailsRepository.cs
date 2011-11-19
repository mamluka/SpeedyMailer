using System.Collections.Generic;

namespace SpeedyMailer.Core.Emails
{
    public interface IEmailsRepository
    {
        void Store(Email email);
        void Store(List<Email> emails);
    }
}