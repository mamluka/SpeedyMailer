using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;
using Ploeh.AutoFixture;
using FluentAssertions;
namespace SpeedyMailer.Master.Web.IntergrationTests.Commands
{
    [TestFixture]
    public class SendCreativeCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenACreativeId_CreateCreativeFragmentsUsingServiceApi()
        {
            var listId = UI.ExecuteCommand<CreateListCommand, string>(x =>
                                                             {
                                                                 x.Name = "MyList";
                                                             });

            UI.ExecuteCommand<AddContactsCommand,long>(x =>
                                                           {
                                                               x.ListId = listId;
                                                               x.Contacts = Fixture.CreateMany<Contact>(2000);
                                                           });

            const string templateBody = "Body";
            var templateId = UI.ExecuteCommand<CreateTemplateCommand,string>(x =>
                                                                                 {
                                                                                     x.Body = templateBody;
                                                                                 });

            var creativeId = UI.ExecuteCommand<AddCreativeCommand, string>(x =>
                                                                               {
                                                                                   x.Body = "body";
                                                                                   x.Lists =  new List<string> { listId};
                                                                                   x.Subject = "Subject";
                                                                                   x.UnsubscribeTemplateId = templateId;
                                                                               });

            Service.Start();
            UI.ExecuteCommand<SendCreativeCommand>(x =>
                                                        {
                                                            x.CreativeId = creativeId;
                                                        });
            Service.Stop();

            var result = Query<CreativeFragment>(x => x.Creative.Id == creativeId);

            result.Should().HaveCount(2);
            result.First().Recipients.Should().HaveCount(1000);
            result.Second().Recipients.Should().HaveCount(1000);

            result.First().UnsubscribeTemplate.Should().Be(templateBody);
            result.Second().UnsubscribeTemplate.Should().Be(templateBody);
        }
    }
}
