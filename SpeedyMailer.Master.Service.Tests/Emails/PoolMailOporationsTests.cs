using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Bridge.Communication;
using SpeedyMailer.Bridge.Model.Drones;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Master.Service.Core.Emails;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;

namespace SpeedyMailer.Master.Service.Tests.Emails
{
    [TestFixture]
    public class PoolMailOporationsTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Perform_ShouldLoadTheFragmentFromTheStoreBeforeTheUpdateWhenProcessingSetAsComplete()
        {
            //Arrange

            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var oporation = new FragmentCompleteOporation
                                {
                                    FragmentOporationType = PoolFragmentOporationType.SetAsCompleted,
                                    FragmentId = Fixture.CreateAnonymous<string>(),
                                    CompletedBy = mailDrone
                                };
            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Load<EmailFragment>(Arg<string>.Is.Equal(oporation.FragmentId))).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var emailOps = new PoolMailOporations(store);
            //Act
            emailOps.Preform(oporation);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void
            Perform_ShouldUpdateTheFragmentWithTheDroneWhoCompletedItAndUpdateTheStatusToCompletedWhenProcessingSetAsComplete
            ()
        {
            //Arrange

            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var oporation = new FragmentCompleteOporation
                                {
                                    FragmentOporationType = PoolFragmentOporationType.SetAsCompleted,
                                    FragmentId = Fixture.CreateAnonymous<string>(),
                                    CompletedBy = mailDrone
                                };
            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m => m.Status == FragmentStatus.Completed &&
                                                                        m.CompletedBy == mailDrone
                                            ))).Repeat.Once();

            session.Stub(x => x.Load<EmailFragment>(Arg<string>.Is.Anything)).Return(new EmailFragment());

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var emailOps = new PoolMailOporations(store);
            //Act
            emailOps.Preform(oporation);
            //Assert
            session.VerifyAllExpectations();
        }
    }
}