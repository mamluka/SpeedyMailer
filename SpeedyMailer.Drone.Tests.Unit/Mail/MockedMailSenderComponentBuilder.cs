using SpeedyMailer.Drone.Mail;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Mail
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