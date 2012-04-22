using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.IntegrationTests.Container
{
    [TestFixture]
    public class ContainerBootstrapperTests : AutoMapperAndFixtureBase
    {
    }
}
