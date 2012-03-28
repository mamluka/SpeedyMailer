using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Raven.Client;
using Rhino.Mocks;
using FluentAssertions;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Tests.Maps;
using SpeedyMailer.Domain.DataAccess.Lists;
using SpeedyMailer.Domain.Model.Lists;
using SpeedyMailer.Tests.Core;
using SpeedyMailer.Tests.Core.DB;
using System.Linq;


namespace SpeedyMailer.Core.Tests.Lists
{
    [TestFixture]
    public class ListRepositoryTests : AutoMapperAndFixtureBase<AutoMapperMaps>
    {
        [Test]
        public void Lists_ShouldLoadTheListCollectionFromTheStore()
        {
            //Arrange
            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Load <ListsStore>("system/lists")).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

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

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            var list = listRepository.Lists();
            //Assert
            list.Lists.Should().HaveCount(0);

        }

        [Test]
        public void Add_ShouldLoadTheCurrentListCollectionBeforeAdding()
        {
            var newList = Fixture.Build<ListDescriptor>().Without(x=> x.Id).CreateAnonymous();

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Expect(x => x.Load<ListsStore>("system/lists")).Repeat.Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Add(newList);
            //Assert
            session.VerifyAllExpectations();

        }

        [Test]
        public void Add_ShouldStoreAListThatContainsTheNewList()
        {
            var newList = Fixture.Build<ListDescriptor>().Without(x => x.Id).CreateAnonymous();

            var listCollection = new ListsStore()
                                     {
                                         Lists = new List<ListDescriptor>()
                                     };

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Stub(x => x.Load<ListsStore>("system/lists")).Return(listCollection);
            session.Expect(x => x.Store(Arg<ListsStore>.Matches(m => m.Lists.Last() == newList),Arg<string>.Is.Equal("system/lists"))).Repeat.Once();


            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Add(newList);
            //Assert
            session.VerifyAllExpectations();

        }

        [Test]
        public void Remove_ShouldRemoveTheListFromTheStore()
        {
            var listToBeDeleted = Fixture.Build<ListDescriptor>().CreateAnonymous();
            var listCollection = new ListsStore()
                               {

                                   Lists = new List<ListDescriptor>() {listToBeDeleted}
                               };
            

        var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Stub(x => x.Load<ListsStore>("system/lists")).Return(listCollection);

            session.Expect(x => x.Store(Arg<ListsStore>.Matches(m => m.Lists.All(p => p.Id != listToBeDeleted.Id)),Arg<string>.Is.Equal("system/lists")))
                .Repeat
                .Once();

           

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Remove(listToBeDeleted.Id);
            //Assert
            session.VerifyAllExpectations();

        }

        [Test]
        public void Update_ShouldUpdateTheListInTheStore()
        {
            var originalList = Fixture.Build<ListDescriptor>().CreateAnonymous();

            var listToBeUpdated = originalList;
            listToBeUpdated.Name = "new Name";

            var listCollection = new ListsStore()
            {

                Lists = new List<ListDescriptor>() { originalList }
            };

            var session = MockRepository.GenerateMock<IDocumentSession>();
            session.Stub(x => x.Load<ListsStore>("system/lists")).Return(listCollection);

            session.Expect(x => x.Store(Arg<ListsStore>.Matches(m => m.Lists.Contains(listToBeUpdated)), Arg<string>.Is.Equal("system/lists")))
                .Repeat
                .Once();

            var store = DocumentStoreFactory.CreateDocumentStoreWithSession(session);

            var listRepository = new ListRepository(store);
            //Act
            listRepository.Update(originalList);
            //Assert
            session.VerifyAllExpectations();

        }


    }
}