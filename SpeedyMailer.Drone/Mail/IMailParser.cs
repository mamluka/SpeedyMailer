using SpeedyMailer.Bridge.Model.Fragments;

namespace SpeedyMailer.Master.Web.UI.Mail
{
    public interface IMailParser
    {
        string Parse(ExtendedRecipient recipient);
        void Initialize(MailParserInitializer mailParserInitializer);
    }
}