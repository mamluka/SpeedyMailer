using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.Master.Web.UI.Mail
{
    public class MailParser : IMailParser
    {
        private readonly IEmailSourceWeaver weaver;
        private MailParserInitializer mailParserInitializer;

        public MailParser(IEmailSourceWeaver weaver)
        {
            this.weaver = weaver;
        }

        #region IMailParser Members

        public string Parse(ExtendedRecipient recipient)
        {
            string body = weaver.WeaveDeals(mailParserInitializer.Body, recipient.DealUrl);

            return body;
        }

        public void Initialize(MailParserInitializer mailParserInitializer)
        {
            this.mailParserInitializer = mailParserInitializer;
        }

        #endregion
    }
}