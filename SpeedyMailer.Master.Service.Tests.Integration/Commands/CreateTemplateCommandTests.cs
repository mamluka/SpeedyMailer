using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Commands
{
    [TestFixture]
    public class CreateTemplateCommandTests : IntegrationTestBase
    {
        [Test]
        public void Execute_WhenGivenATemplateBody_ShouldSaveTheTemplateInTheStore()
        {
            const string templateBody = "template body";
            var templateId = UIActions.ExecuteCommand<CreateTemplateCommand, string>(x =>
                                                                 {
                                                                     x.Body = templateBody;
																	 x.Type = TemplateType.Unsubscribe;
                                                                 });

            var result = Load<Template>(templateId);

            result.Body.Should().Be(templateBody);
            result.Type.Should().Be(TemplateType.Unsubscribe);
        }
    }
}
