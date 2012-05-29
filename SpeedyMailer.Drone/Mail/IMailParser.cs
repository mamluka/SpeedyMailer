using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Drone.Mail
{
    public interface IMailParser
    {
        string Parse(ExtendedRecipient recipient);
        void Initialize(MailParserInitializer mailParserInitializer);
    }
}