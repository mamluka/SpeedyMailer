using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Rhino.Mocks;
using SpeedyMailer.Core.Commands;
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

            var creativeId = UI.ExecuteCommand<AddCreativeCommand, string>(x =>
                                                                               {
                                                                                   x.Body = "Body";
                                                                                   x.Lists =  new List<string> { listId};
                                                                                   x.Subject = "Subject";
                                                                               });

            var templateId = UI.ExecuteCommand<CreateTemplateCommand>(x =>
                                                                          {
                                                                              x.Body = "body";
                                                                          });
            Service.Start();
            UI.ExecuteCommand<SendCreativeCommand>(x =>
                                                        {
                                                            x.CreativeId = creativeId;
                                                            x.UnsubscribedTemplateId = "tempId";
                                                        });
            Service.Stop();

            var result = Query<CreativeFragment>(x => x.Creative.Id == creativeId);

            result.Should().HaveCount(2);
            result.First().Recipients.Should().HaveCount(1000);
            result.Second().Recipients.Should().HaveCount(1000);

        }
    }

    public class CreateTemplateCommand:Command<string>
    {
        public override string Execute()
        {
            return null;
        }
    }
}
