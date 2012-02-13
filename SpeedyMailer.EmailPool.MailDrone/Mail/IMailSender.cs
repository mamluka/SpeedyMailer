using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.EmailPool.MailDrone.Mail
{
    public interface IMailSender
    {
        void ProcessFragment(EmailFragment fragment);
    }
}