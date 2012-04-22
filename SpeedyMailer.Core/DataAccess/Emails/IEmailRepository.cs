using SpeedyMailer.Core.Domain.Emails;

namespace SpeedyMailer.Core.DataAccess.Emails
{
    public interface IEmailRepository
    {
        void Store(Email email);
    }
}