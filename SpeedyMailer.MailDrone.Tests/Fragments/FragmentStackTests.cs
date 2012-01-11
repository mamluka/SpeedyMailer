using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.EmailPool.Master.Pool;
using SpeedyMailer.MailDrone.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;

namespace SpeedyMailer.MailDrone.Tests.Fragments
{
    [TestFixture]
    public class FragmentStackTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Pop_ShouldLoadTheBaseURLToTheRestClient()
        {
            //Arrange
            var baseUrl = Fixture.CreateAnonymous<string>();

            var configurationManager = MockRepository.GenerateStub<IConfigurationManager>();
            configurationManager.BasePoolUrl = baseUrl;

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.BaseUrl).SetPropertyWithArgument(baseUrl);
            
            
            

            var fragmentSlide = new FragmentStack(restClient, configurationManager);
            //Act
            fragmentSlide.Pop();
            //Assert
            restClient.VerifyAllExpectations();

        }

        [Test]
        public void Pop_ShouldLoadThePopFragmentUrlIntoTheRequest()
        {
            //Arrange
            var fragmentUrl = Fixture.CreateAnonymous<string>();

            var configurationManager = MockRepository.GenerateStub<IConfigurationManager>();
            configurationManager.PoolOporationsUrls.PopFragmentUrl = fragmentUrl;

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.Execute<FragmentResponse>()




            var fragmentSlide = new FragmentStack(restClient, configurationManager);
            //Act
            fragmentSlide.Pop();
            //Assert
            restClient.VerifyAllExpectations();

        }
    }

    public class FragmentStack
    {
        private readonly IRestClient restClient;
        private readonly IConfigurationManager configurationManager;

        public FragmentStack(IRestClient restClient, IConfigurationManager configurationManager)
        {
            this.restClient = restClient;
            this.configurationManager = configurationManager;
        }

        public EmailFragment Pop()
        {
            restClient.BaseUrl = configurationManager.BasePoolUrl;
            return  new EmailFragment();
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
