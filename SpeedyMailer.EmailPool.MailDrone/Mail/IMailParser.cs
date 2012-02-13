using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.EmailPool.MailDrone.Mail
{
    public interface IMailParser
    {
        string Parse(ExtendedRecipient recipient);
        void Initialize(MailParserInitializer mailParserInitializer);
    }
}