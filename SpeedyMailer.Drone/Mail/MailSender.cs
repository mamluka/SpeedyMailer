using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Master.Web.UI.Mail
{
    public class MailSender : IMailSender
    {
        private readonly IMailParser mailParser;

        public MailSender(IMailParser mailParser)
        {
            this.mailParser = mailParser;
        }

        public void ProcessFragment(EmailFragment fragment)
        {

            mailParser.Initialize(new MailParserInitializer()
                                      {
                                          Body = fragment.Body,
                                          MailId = fragment.MailId,
                                          UnsubscribeTemplate = fragment.UnsubscribeTemplate
                                      });

            foreach (var recipient in fragment.ExtendedRecipients)
            {
                var body = mailParser.Parse(recipient);
            }
        }
    }
}