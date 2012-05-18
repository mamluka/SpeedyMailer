using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.DataAccess.Fragments;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.Unit.Base;
using SpeedyMailer.Tests.Core.Unit.Database;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class FragmentRepositoryTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Add_ShouldAddTheFragementToTheStore()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<EmailFragment>();
            var session = MockRepository.GenerateMock<IDocumentSession>();

            session.Expect(x => x.Store(Arg<EmailFragment>.Is.Equal(fragment))).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var fragmentRepository = new FragmentRepository(store);
            //Act
            fragmentRepository.Add(fragment);
            //Assert
            session.VerifyAllExpectations();
        }
    }
}