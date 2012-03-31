using SpeedyMailer.Master.Web.UI.Mail;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Drone.Tests.Mail
{
    internal class MockedMailSenderComponentBuilder : IMockedComponentBuilder<MailSender>
    {
        public IMailParser MailParser { get; set; }

        #region IMockedComponentBuilder<MailSender> Members

        public MailSender Build()
        {
            return new MailSender(MailParser);
        }

        #endregion
    }
}