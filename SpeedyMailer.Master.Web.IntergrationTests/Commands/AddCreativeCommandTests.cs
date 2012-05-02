using System.Collections.Generic;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;
using FluentAssertions;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands
{
    [TestFixture]
    public class AddCreativeCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenAllCreateParametersAreValid_ShouldAddCreativeToStore()
        {
            var body = "Body";
            var subject = "Subject";

            var lists = new List<string>
                            {
                                "list1",
                                "list2"
                            };

            var creativeId = Master.ExecuteCommand<AddCreativeCommand, string>(x =>
                                                                                   {
                                                                                       x.Subject = subject;
                                                                                       x.Body = body;

                                                                                       x.Subject = subject;
                                                                                       x.Body = body;

                                                                                       x.Lists = lists;
                                                                                   });

            var result = Load<Creative>(creativeId);

            result.Body.Should().Be(body);
            result.Subject.Should().Be(subject);
            result.Lists.Should().Contain(lists);
        }
    }
}
