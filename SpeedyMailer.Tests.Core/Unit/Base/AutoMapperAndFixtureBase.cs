using NUnit.Framework;
using Ploeh.AutoFixture;

namespace SpeedyMailer.Tests.Core.Unit.Base
{
    public class AutoMapperAndFixtureBase
    {
        protected Fixture Fixture;

        [TestFixtureSetUp]
        public void Initialize()
        {
            Fixture = new Fixture();
        }
    }
}