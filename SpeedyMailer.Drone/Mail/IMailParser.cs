using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Master.Web.UI.Mail
{
    public interface IMailParser
    {
        string Parse(ExtendedRecipient recipient);
        void Initialize(MailParserInitializer mailParserInitializer);
    }
}