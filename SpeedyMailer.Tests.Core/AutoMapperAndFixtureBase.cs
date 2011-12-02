using AutoMapper;
using NUnit.Framework;
using Ploeh.AutoFixture;

namespace SpeedyMailer.Tests.Core
{
    public class AutoMapperAndFixtureBase<T> where T:IAutoMapperMaps, new()
    {
        protected Fixture Fixture;
        protected IMappingEngine Mapper;

        [TestFixtureSetUp]
        public void Initialize()
        {
            Fixture = new Fixture();
            var mapCreator = new T();
            mapCreator.CreateMaps();
            Mapper =  AutoMapper.Mapper.Engine;
        }
    }
}