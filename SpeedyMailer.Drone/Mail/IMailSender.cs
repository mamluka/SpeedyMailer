using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Master.Web.UI.Mail
{
    public interface IMailSender
    {
        void ProcessFragment(EmailFragment fragment);
    }
}