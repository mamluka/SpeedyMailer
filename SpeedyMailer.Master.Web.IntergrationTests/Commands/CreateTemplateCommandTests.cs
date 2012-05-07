using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Web.Core.Commands;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Web.IntergrationTests.Commands
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
