using SpeedyMailer.Master.Web.UI.Mail;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Mail
{
    internal class MockedMailSenderComponentBuilder : IMockedComponentBuilder<MailSender>
    {
        public IMailParser MailParser { get; set; }


        public MailSender Build()
        {
            return new MailSender(MailParser);
        }

    }
}