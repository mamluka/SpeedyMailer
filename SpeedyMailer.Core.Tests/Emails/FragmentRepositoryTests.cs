using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using SpeedyMailer.Bridge.Model.Fragments;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Core.Emails;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Domain.DataAccess.Fragments;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.DB;

namespace SpeedyMailer.Core.Tests.Emails
{
    [TestFixture]
    public class FragmentRepositoryTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Add_ShouldAddTheFragementToTheStore()
        {
            //Arrange
            var fragment = Fixture.CreateAnonymous<EmailFragment>();
            var session = MockRepository.GenerateMock<IDocumentSession>();

            session.Expect(x => x.Store(Arg<EmailFragment>.Is.Equal(fragment))).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var fragmentRepository = new FragmentRepository(store);
            //Act
            fragmentRepository.Add(fragment);
            //Assert
            session.VerifyAllExpectations();
            
        }
    }
}
