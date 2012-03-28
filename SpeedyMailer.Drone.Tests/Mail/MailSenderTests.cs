using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Drone.Tests.Maps;
using SpeedyMailer.Master.Web.UI.Mail;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;

namespace SpeedyMailer.Drone.Tests.Mail
{
    [TestFixture]
    public class MailSenderTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Send_ShouldParseEachMailInTheFragment()
        {
            //Arrange
            var fragment = Fixture.Build<EmailFragment>().With(x => x.ExtendedRecipients, Fixture.CreateMany<ExtendedRecipient>(3).ToList()).CreateAnonymous();

            var mailParser = MockRepository.GenerateMock<IMailParser>();
            mailParser.Expect(x => x.Parse(Arg<ExtendedRecipient>.Is.Anything)).Repeat.Times(3).Return("body");

            var builder = new MockedMailSenderComponentBuilder();
            builder.MailParser = mailParser;

            var mailSender = builder.Build();
            //Act
            mailSender.ProcessFragment(fragment);
            //Assert
            mailParser.VerifyAllExpectations();
            

        }

        [Test]
        public void Send_ShouldInitializeTheParserWithTheRightParameters()
        {
            //Arrange
            var fragment = Fixture.Build<EmailFragment>().With(x => x.ExtendedRecipients, Fixture.CreateMany<ExtendedRecipient>(3).ToList()).CreateAnonymous();

            var mailParser = MockRepository.GenerateMock<IMailParser>();
            mailParser.Expect(
                x =>
                x.Initialize(
                    Arg<MailParserInitializer>.Matches(
                        m =>
                        m.MailId == fragment.MailId && 
                        m.Body == fragment.Body &&
                        m.UnsubscribeTemplate == fragment.UnsubscribeTemplate
                        ))).Repeat.Once();

            var builder = new MockedMailSenderComponentBuilder();
            builder.MailParser = mailParser;

            var mailSender = builder.Build();
            //Act
            mailSender.ProcessFragment(fragment);
            //Assert
            mailParser.VerifyAllExpectations();
        }
    }

    class MockedMailSenderComponentBuilder:IMockedComponentBuilder<MailSender>
    {
        public IMailParser MailParser { get; set; }
        public MailSender Build()
        {
            return new MailSender(MailParser);
        }
    }
}
