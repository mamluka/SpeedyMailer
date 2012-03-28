using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Drone.Tests.Maps;
using SpeedyMailer.Master.Web.UI.Mail;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using SpeedyMailer.Tests.Core.Emails;

namespace SpeedyMailer.Drone.Tests.Mail
{
    [TestFixture]
    public class MailParserTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Parse_ShouldReplaceAllOfferLinksWithPersonalizedOfferLinks()
        {
            //Arrange
            var body = EmailSourceFactory.StandardEmail();
            var init = Fixture.Build<MailParserInitializer>().With(x=> x.Body,body).CreateAnonymous();
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

        [Test]
        public void Parse_ShouldAddTheUnsubscribeTemplateWithTheUrlToTheBody()
        {
            //Arrange
            var body = EmailSourceFactory.StandardEmail();
            var init = Fixture.Build<MailParserInitializer>().With(x => x.Body, body).CreateAnonymous();
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

    }
}
