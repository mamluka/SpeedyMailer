using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SpeedyMailer.Core.Api;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Master.Web.Tests.Integration.Commands
{
    [TestFixture]
    public class SendCreativeCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenACreativeId_CreateCreativeFragmentsUsingServiceApi()
        {
            var listId = UIActions.CreateListWithRandomContacts("MyList", 1500);

            const string templateBody = "Body";
            var templateId = UIActions.ExecuteCommand<CreateTemplateCommand,string>(x =>
                                                                                 {
                                                                                     x.Body = templateBody;
                                                                                 });

            var creativeId = UIActions.ExecuteCommand<AddCreativeCommand, string>(x =>
                                                                               {
                                                                                   x.Body = "body";
                                                                                   x.Lists =  new List<string> { listId};
                                                                                   x.Subject = "Subject";
                                                                                   x.UnsubscribeTemplateId = templateId;
                                                                               });

			ServiceActions.Initialize();
            ServiceActions.Start();
			

            UIActions.ExecuteCommand<SendCreativeCommand,ApiResult>(x =>
                                                             	{
                                                             		x.CreativeId = creativeId;
                                                             	});
           
        	WaitForEntityToExist<CreativeFragment>(x => x.Creative.Id == creativeId,2);
			ServiceActions.Stop();

        	var result = Query<CreativeFragment>().Where(x => x.Creative.Id == creativeId);

            result.Should().HaveCount(2);
            result.First().Recipients.Should().HaveCount(1000);
            result.Second().Recipients.Should().HaveCount(500);

            result.First().UnsubscribeTemplate.Should().Be(templateBody);
            result.Second().UnsubscribeTemplate.Should().Be(templateBody);
        }
    }
}
