using SpeedyMailer.Domain.Model.Emails;

namespace SpeedyMailer.Domain.DataAccess.Emails
{
    public interface IEmailRepository
    {
        void Store(Email email);
    }
}