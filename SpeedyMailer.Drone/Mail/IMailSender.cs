using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Drone.Mail
{
    public interface IMailSender
    {
        void ProcessFragment(EmailFragment fragment);
    }
}