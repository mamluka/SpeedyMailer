using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using EqualityComparer;
using NUnit.Framework;
using Ninject;
using Ninject.Activation.Strategies;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Rhino.Mocks;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Web.Core;
using SpeedyMailer.Master.Web.UI;
using SpeedyMailer.Master.Web.UI.Bootstrappers;
using SpeedyMailer.Tests.Core.Unit.Base;
using Raven.Client.Linq;
using System.Linq;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    [TestFixture]
    public class IntegrationTestBase : AutoMapperAndFixtureBase
    {
        public IKernel DroneKernel { get; private set; }
        public IKernel MasterKernel { get; private set; }
        public IDocumentStore DocumentStore { get; private set; }

        public Actions Drone { get; set; }
        public Actions Master { get; set; }

        [TestFixtureSetUp]
        public void FixtureSetup()
        {

            DocumentStore = MockRepository.GenerateStub<IDocumentStore>();

            Bootstrap(MasterKernel);

            MasterKernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (CoreAssemblyMarker),
                                                            typeof (WebCoreAssemblyMarker),
                                                            typeof(ServiceAssemblyMarker)
                                                        }))
                .BindInterfaceToDefaultImplementation()
                .Storage<IDocumentStore>(x => x.Constant(DocumentStore))
                .Settings(x => x.UseDocumentDatabase())
                .Done();

            DroneKernel = ContainerBootstrapper
                .Bootstrap()
                .Analyze(x => x.AssembiesContaining(new[]
                                                        {
                                                            typeof (DroneAssemblyMarker),
                                                        }))
                .BindInterfaceToDefaultImplementation()
                .NoDatabase()
                .Settings(x => x.UseJsonFiles())
                .Done();

            Master = new Actions(MasterKernel);
            Drone = new Actions(DroneKernel);
        }

        [SetUp]
        public void Setup()
        {
            var embeddableDocumentStore = new EmbeddableDocumentStore { 
                RunInMemory = true,
                Conventions = new DocumentConvention
                {
                    DefaultQueryingConsistency = ConsistencyOptions.QueryYourWrites,
                }
                
            };
            DocumentStore = embeddableDocumentStore.Initialize();

            MasterKernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);
            DroneKernel.Rebind<IDocumentStore>().ToConstant(DocumentStore);
        }

        public void Store(object item)
        {
            using (var session = DocumentStore.OpenSession())
            {
                session.Store(item);
                session.SaveChanges();
            }
        }


        public T MasterResolve<T>()
        {
            return MasterKernel.Get<T>();
        }

        public T DroneResolve<T>()
        {
            return DroneKernel.Get<T>();
        }

        public bool Compare<T>(T first, T second)
        {
            return MemberComparer.Equal(first, second);
        }

        public T Load<T>(string id)
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Load<T>(id);
            }
        }

        public IList<T> Query<T>(Expression<Func<T,bool>> expression )
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Query<T>().Where(expression).Customize(x=> x.WaitForNonStaleResults()).ToList();
            }
        }

        public IList<T> Query<T>()
        {
            using (var session = DocumentStore.OpenSession())
            {
                return session.Query<T>().Customize(x => x.WaitForNonStaleResults()).ToList();
            }
        }
    }
}