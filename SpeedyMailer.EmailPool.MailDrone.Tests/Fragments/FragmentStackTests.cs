using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using RestSharp;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Protocol;
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

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.BaseUrl).SetPropertyWithArgument(baseUrl);

            

            var builder = new MockedFragmentStackBuilder(Fixture);
            builder.RestClient = restClient;

            var fragmentSlide = builder.WithBasePoolUrl(baseUrl).AddResponseMockedObject().Build();
            //Act
            fragmentSlide.Pop();
            //Assert
            restClient.VerifyAllExpectations();

        }

        [Test]
        public void Pop_ShouldLoadThePopFragmentUrlIntoTheRequestAndExecuteTheRequest()
        {
            //Arrange
            var fragmentUrl = Fixture.CreateAnonymous<string>();

            var fragmentResponse = Fixture.CreateAnonymous<FragmentResponse>();
            var restResponse = MockRepository.GenerateStub<RestResponse<FragmentResponse>>();
            restResponse.Data = fragmentResponse;

            var restClient = MockRepository.GenerateMock<IRestClient>();
            restClient.Expect(x => x.Execute<FragmentResponse>(Arg<RestRequest>.Matches(m =>
                                                                                        m.Resource == fragmentUrl)))
                .Repeat.Once().Return(restResponse);

            var builder = new MockedFragmentStackBuilder(Fixture);
            builder.RestClient = restClient;

            var fragmentSlide = builder.WithPopFragmentUrl(fragmentUrl).Build();
            //Act
            fragmentSlide.Pop();
            //Assert
            restClient.VerifyAllExpectations();

        }

        [Test]
        public void Pop_ShouldDeserializeTheResponseAndReturnTheFragment()
        {
            //Arrange
            var fragmentResponse = Fixture.CreateAnonymous<FragmentResponse>();
            var restResponse = MockRepository.GenerateStub<RestResponse<FragmentResponse>>();

            restResponse.Data = fragmentResponse;

            var restClient = MockRepository.GenerateStub<IRestClient>();
            restClient.Stub(x => x.Execute<FragmentResponse>(Arg<RestRequest>.Is.Anything)).Return(restResponse);

            var builder = new MockedFragmentStackBuilder(Fixture);
            builder.RestClient = restClient;
            //Act
            var fragmentSlide = builder.Build();
            //Assert
            var fragment = fragmentSlide.Pop();

            fragment.Should().BeSameAs(fragmentResponse.EmailFragment);
        } 



    }

    public class MockedFragmentStackBuilder : IMockedComponentBuilder<FragmentStack>
    {
        private readonly IFixture fixture;
        public IRestClient RestClient { get; set; }
        public IConfigurationManager ConfigurationManager { get; set; }

        public MockedFragmentStackBuilder(IFixture fixture)
        {

            this.fixture = fixture;

           ConfigurationManager = MockRepository.GenerateStub<IConfigurationManager>();
            ConfigurationManager.BasePoolUrl = "baseurl";
            ConfigurationManager.PoolOporationsUrls =
                fixture.Build<PoolOporationsUrls>().With(x => x.PopFragmentUrl, "fragmenturl").CreateAnonymous();
        }

        public MockedFragmentStackBuilder WithBasePoolUrl(string url)
        {
            ConfigurationManager.BasePoolUrl = url;
            return this;
        }

        public MockedFragmentStackBuilder WithPopFragmentUrl(string url)
        {
            ConfigurationManager.PoolOporationsUrls =
                fixture.Build<PoolOporationsUrls>().With(x => x.PopFragmentUrl, url).CreateAnonymous();
            return this;
        }

        public MockedFragmentStackBuilder AddResponseMockedObject()
        {
            var fragmentResponse = fixture.CreateAnonymous<FragmentResponse>();
            var restResponse = MockRepository.GenerateStub<RestResponse<FragmentResponse>>();
            restResponse.Data = fragmentResponse;

            RestClient.Stub(x => x.Execute<FragmentResponse>(Arg<RestRequest>.Is.Anything)).Return(restResponse);
            return this;
        }

        public FragmentStack Build()
        {
            return new FragmentStack(RestClient,ConfigurationManager);
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

            var request = new RestRequest()
                              {
                                  Resource = configurationManager.PoolOporationsUrls.PopFragmentUrl
                              };

            var response = restClient.Execute<FragmentResponse>(request);

            return response.Data.EmailFragment;
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
