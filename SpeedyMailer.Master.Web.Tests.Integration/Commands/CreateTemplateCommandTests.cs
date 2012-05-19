using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Web.Core.Commands;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.Tests.Integration.Commands
{
    [TestFixture]
    public class CreateTemplateCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenATemplateBody_ShouldSaveTheTemplateInTheStore()
        {
            const string templateBody = "template body";
            var templateId = UI.ExecuteCommand<CreateTemplateCommand, string>(x =>
                                                                 {
                                                                     x.Body = templateBody;
                                                                 });

            var result = Load<CreativeTemplate>(templateId);

            result.Body.Should().Be(templateBody);
        }
    }
}
