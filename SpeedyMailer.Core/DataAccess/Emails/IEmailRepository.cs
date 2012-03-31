using SpeedyMailer.Domain.Model.Emails;

namespace SpeedyMailer.Core.DataAccess.Emails
{
    public interface IEmailRepository
    {
        void Store(Email email);
    }
}