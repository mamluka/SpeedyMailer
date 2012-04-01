using SpeedyMailer.Core.Emails;
using SpeedyMailer.Tests.Core;

namespace SpeedyMailer.Core.Tests.Emails
{
    public class MockedEmailSourceWeaverBuilder : IMockedComponentBuilder<EmailSourceWeaver>
    {

        public EmailSourceWeaver Build()
        {
            return new EmailSourceWeaver();
        }

    }
}