using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
    [TestFixture]
    public class CreateListCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenCalled_ShouldCreateAList()
        {
            var id = UiActions.ExecuteCommand<CreateListCommand, string>(x =>
                                                                 {
                                                                     x.Name = "Default";
                                                                 });

            var result = Store.Load<ListDescriptor>(id);

            result.Name.Should().Be("Default");
        }
    }
}
