using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.Tests.Integration.Commands
{
    [TestFixture]
    public class CreateListCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenCalled_ShouldCreateAList()
        {
            var id = UI.ExecuteCommand<CreateListCommand, string>(x =>
                                                                 {
                                                                     x.Name = "Default";
                                                                 });

            var result = Load<ListDescriptor>(id);

            result.Name.Should().Be("Default");
        }
    }
}
