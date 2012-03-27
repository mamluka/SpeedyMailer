using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Master.Web.UI.Mail
{
    public interface IMailSender
    {
        void ProcessFragment(EmailFragment fragment);
    }
}