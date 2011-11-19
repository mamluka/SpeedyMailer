using AutoMapper;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Tests.Maps;

namespace SpeedyMailer.Core.Tests
{
    public class AutoMapperAndFixtureBase
    {
        protected Fixture Fixture;
        protected IMappingEngine Mapper;

        [TestFixtureSetUp]
        public void Initialize()
        {
            Fixture = new Fixture();
            AutoMapperMaps.CreateMaps();
            Mapper = AutoMapper.Mapper.Engine;
        }
    }
}