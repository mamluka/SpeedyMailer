using AutoMapper;
using Bootstrap;
using Bootstrap.AutoMapper;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Mapping;

namespace SpeedyMailer.Tests.Core.Unit.Base
{
    public class AutoMapperAndFixtureBase
    {
        protected Fixture Fixture;
        protected IMappingEngine Mapper;

        [TestFixtureSetUp]
        public void Initialize()
        {
            Fixture = new Fixture();
            Mapper = AutoMapper.Mapper.Engine;
        }
    }
}