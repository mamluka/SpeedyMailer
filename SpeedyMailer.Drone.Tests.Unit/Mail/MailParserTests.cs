using NUnit.Framework;
using Ploeh.AutoFixture;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Master.Web.UI.Mail;
using SpeedyMailer.Tests.Core.Emails;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Drone.Tests.Unit.Mail
{
    [TestFixture]
    public class MailParserTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Parse_ShouldAddTheUnsubscribeTemplateWithTheUrlToTheBody()
        {
            //Arrange
            string body = EmailSourceFactory.StandardEmail();
            MailParserInitializer init =
                Fixture.Build<MailParserInitializer>().With(x => x.Body, body).CreateAnonymous();
            var recipient = Fixture.CreateAnonymous<ExtendedRecipient>();

            var weaver = MockRepository.GenerateMock<IEmailSourceWeaver>();
            weaver.Expect(
                x =>
                x.WeaveUnsubscribeTemplate(Arg<string>.Is.Equal(body), Arg<string>.Is.Equal(init.UnsubscribeTemplate),
                                           Arg<string>.Is.Equal(recipient.UnsubscribeUrl))).Repeat.Once().Return("");


            var parser = new MailParser(weaver);
            //Act

            parser.Initialize(init);
            parser.Parse(recipient);
        }

        [Test]
        public void Parse_ShouldReplaceAllOfferLinksWithPersonalizedOfferLinks()
        {
            //Arrange
            string body = EmailSourceFactory.StandardEmail();
            MailParserInitializer init =
                Fixture.Build<MailParserInitializer>().With(x => x.Body, body).CreateAnonymous();
            var recipient = Fixture.CreateAnonymous<ExtendedRecipient>();

            var weaver = MockRepository.GenerateMock<IEmailSourceWeaver>();
            weaver.Expect(
                x =>
                x.WeaveDeals(Arg<string>.Is.Equal(body),
                             Arg<string>.Is.Equal(recipient.DealUrl)))
                .Repeat.Once().Return("");


            var parser = new MailParser(weaver);
            //Act

            parser.Initialize(init);
            parser.Parse(recipient);

            //Assert
        }
    }
}