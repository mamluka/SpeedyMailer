using NUnit.Framework;
using Raven.Client;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.MailDrones;
using SpeedyMailer.Core.Protocol;
using SpeedyMailer.EmailPool.Core.Emails;
using SpeedyMailer.EmailPool.Master.MailDrones;
using SpeedyMailer.EmailPool.Master.Tests.Maps;
using SpeedyMailer.Tests.Core;
using Ploeh.AutoFixture;
using SpeedyMailer.Tests.Core.DB;
using Rhino.Mocks;

namespace SpeedyMailer.EmailPool.Master.Tests.Emails
{
    [TestFixture]
    public class EmailOporationsTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Perform_ShouldLoadTheFragmentFromTheStoreBeforeTheUpdate()
        {
            //Arrange

            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var oporation = new FragmentComplete()
                                {
                                    FragmentOpotationType = FragmentOpotationType.SetAsCompleted,
                                    FragmentId = Fixture.CreateAnonymous<string>(),
                                    CompletedBy = mailDrone

                                };
            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Load<EmailFragment>(Arg<string>.Is.Equal(oporation.FragmentId))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var emailOps = new MailOporations(store);
            //Act
            emailOps.Preform(oporation);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Perform_ShouldUpdateTheFragmentWithTheDroneWhoCompletedItAndUpdateTheStatusToCompleted()
        {
            //Arrange

            var mailDrone = Fixture.CreateAnonymous<MailDrone>();

            var oporation = new FragmentComplete()
            {
                FragmentOpotationType = FragmentOpotationType.SetAsCompleted,
                FragmentId = Fixture.CreateAnonymous<string>(),
                CompletedBy = mailDrone

            };
            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Store(Arg<EmailFragment>.Matches(m => m.Status == FragmentStatus.Completed &&
                                                                        m.CompletedBy == mailDrone
                                            ))).Repeat.Once();

            session.Stub(x => x.Load<EmailFragment>(Arg<string>.Is.Anything)).Return(new EmailFragment());

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var emailOps = new MailOporations(store);
            //Act
            emailOps.Preform(oporation);
            //Assert
            session.VerifyAllExpectations();

        }
    }
}
