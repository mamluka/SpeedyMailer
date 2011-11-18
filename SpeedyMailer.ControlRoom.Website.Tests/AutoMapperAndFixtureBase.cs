using AutoMapper;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace SpeedyMailer.ControlRoom.Website.Tests
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