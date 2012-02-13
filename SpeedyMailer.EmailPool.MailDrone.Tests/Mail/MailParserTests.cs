using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.EmailPool.MailDrone.Mail;
using SpeedyMailer.MailDrone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Emails;

namespace SpeedyMailer.MailDrone.Tests.Mail
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

    public class MailParser:IMailParser
    {
        private readonly IEmailSourceWeaver weaver;
        private MailParserInitializer mailParserInitializer;

        public MailParser(IEmailSourceWeaver weaver)
        {
            this.weaver = weaver;
        }

        public string Parse(ExtendedRecipient recipient)
        {
            var body = weaver.WeaveDeals(mailParserInitializer.Body, new LeadIdentity()
                                                                         {
                                                                             Address = recipient.Address,
                                                                             EmailId = mailParserInitializer.MailId
                                                                         });

            return body;
        }

        public void Initialize(MailParserInitializer mailParserInitializer)
        {
            this.mailParserInitializer = mailParserInitializer;
        }
    }
}
