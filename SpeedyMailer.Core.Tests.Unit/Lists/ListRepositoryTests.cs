using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Client;
using Rhino.Mocks;
using SpeedyMailer.Core.DataAccess.Lists;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Tests.Core.Unit.Base;
using SpeedyMailer.Tests.Core.Unit.Database;

namespace SpeedyMailer.Core.Tests.Unit.Lists
{
    [TestFixture]
    public class ListRepositoryTests : AutoMapperAndFixtureBase
    {
        [Test]
        public void Add_ShouldLoadTheCurrentListCollectionBeforeAdding()
        {
            ListDescriptor newList = Fixture.Build<ListDescriptor>().Without(x => x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Load<ListsStore>("system/lists")).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Add(newList);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Add_ShouldStoreAListThatContainsTheNewList()
        {
            ListDescriptor newList = Fixture.Build<ListDescriptor>().Without(x => x.Id).CreateAnonymous();

            var listCollection = new ListsStore
                                     {
                                         Lists = new List<ListDescriptor>()
                                     };

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Stub(x => x.Load<ListsStore>("system/lists")).Return(listCollection);
            session.Expect(
                x =>
                x.Store(Arg<ListsStore>.Matches(m => m.Lists.Last() == newList), Arg<string>.Is.Equal("system/lists"))).
                Repeat.Once();


            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Add(newList);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Lists_ShouldLoadTheListCollectionFromTheStore()
        {
            //Arrange
            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Load<ListsStore>("system/lists")).Repeat.Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Lists();
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Lists_ShouldReturnAnEmptyListWhenTheObjectIsEmpty()
        {
            //Arrange
            var session = MockRepository.GenerateStub<IDocumentSession>();
            session.Stub(x => x.Load<ListsStore>("system/lists")).Return(null);

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            ListsStore list = listRepository.Lists();
            //Assert
            list.Lists.Should().HaveCount(0);
        }

        [Test]
        public void Remove_ShouldRemoveTheListFromTheStore()
        {
            ListDescriptor listToBeDeleted = Fixture.Build<ListDescriptor>().CreateAnonymous();
            var listCollection = new ListsStore
                                     {
                                         Lists = new List<ListDescriptor> {listToBeDeleted}
                                     };


            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Stub(x => x.Load<ListsStore>("system/lists")).Return(listCollection);

            session.Expect(
                x =>
                x.Store(Arg<ListsStore>.Matches(m => m.Lists.All(p => p.Id != listToBeDeleted.Id)),
                        Arg<string>.Is.Equal("system/lists")))
                .Repeat
                .Once();


            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Remove(listToBeDeleted.Id);
            //Assert
            session.VerifyAllExpectations();
        }

        [Test]
        public void Update_ShouldUpdateTheListInTheStore()
        {
            ListDescriptor originalList = Fixture.Build<ListDescriptor>().CreateAnonymous();

            ListDescriptor listToBeUpdated = originalList;
            listToBeUpdated.Name = "new Name";

            var listCollection = new ListsStore
                                     {
                                         Lists = new List<ListDescriptor> {originalList}
                                     };

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Stub(x => x.Load<ListsStore>("system/lists")).Return(listCollection);

            session.Expect(
                x =>
                x.Store(Arg<ListsStore>.Matches(m => m.Lists.Contains(listToBeUpdated)),
                        Arg<string>.Is.Equal("system/lists")))
                .Repeat
                .Once();

            IDocumentStore store = DocumentStoreFactory.StubDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Update(originalList);
            //Assert
            session.VerifyAllExpectations();
        }
    }
}