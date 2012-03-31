using SpeedyMailer.Domain.Emails;

namespace SpeedyMailer.Core.Emails
{
    public interface IEmailPoolService
    {
        AddEmailToPoolResults AddEmail(Email email);
    }
}