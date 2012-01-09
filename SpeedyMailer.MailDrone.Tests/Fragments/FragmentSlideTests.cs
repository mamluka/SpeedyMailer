using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.MailDrone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;

namespace SpeedyMailer.MailDrone.Tests.Fragments
{
    [TestFixture]
    public class FragmentSlideTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Pop_ShouldLoadTheBaseURLToTheRestClient()
        {
            //Arrange
            var configurationManager = MockRepository.GenerateStub<IConfigurationManager>()
            //Act

            //Assert

        }
    }

    public interface IConfigurationManager
    {
        string BasePoolUrl { get; set; }
        PoolOporationsUrls PoolOporationsUrls { get; set; }

    }

    public class PoolOporationsUrls
    {
        public string PopFragmentUrl { get; set; }

    }
}
