using SpeedyMailer.Bridge.Model.Fragments;

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
            mailParser.Initialize(new MailParserInitializer
                                      {
                                          Body = fragment.Body,
                                          MailId = fragment.MailId,
                                          UnsubscribeTemplate = fragment.UnsubscribeTemplate
                                      });

            foreach (ExtendedRecipient recipient in fragment.ExtendedRecipients)
            {
                string body = mailParser.Parse(recipient);
            }
        }

    }
}