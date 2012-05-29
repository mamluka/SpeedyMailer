using System.Linq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Drone.Mail;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Mail
{
    [TestFixture]
    public class MailSenderTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Send_ShouldInitializeTheParserWithTheRightParameters()
        {
            //Arrange
            EmailFragment fragment =
                Fixture.Build<EmailFragment>().With(x => x.ExtendedRecipients,
                                                    Fixture.CreateMany<ExtendedRecipient>(3).ToList()).CreateAnonymous();

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

            MailSender mailSender = builder.Build();
            //Act
            mailSender.ProcessFragment(fragment);
            //Assert
            mailParser.VerifyAllExpectations();
        }

        [Test]
        public void Send_ShouldParseEachMailInTheFragment()
        {
            //Arrange
            EmailFragment fragment =
                Fixture.Build<EmailFragment>().With(x => x.ExtendedRecipients,
                                                    Fixture.CreateMany<ExtendedRecipient>(3).ToList()).CreateAnonymous();

            var mailParser = MockRepository.GenerateMock<IMailParser>();
            mailParser.Expect(x => x.Parse(Arg<ExtendedRecipient>.Is.Anything)).Repeat.Times(3).Return("body");

            var builder = new MockedMailSenderComponentBuilder();
            builder.MailParser = mailParser;

            MailSender mailSender = builder.Build();
            //Act
            mailSender.ProcessFragment(fragment);
            //Assert
            mailParser.VerifyAllExpectations();
        }
    }
}