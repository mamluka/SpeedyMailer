using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Ploeh.AutoFixture;
using Raven.Client;
using Raven.Client.Embedded;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Integration.Base;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.IntegrationTests.Container
{
	[TestFixture]
	public class ContainerBootstrapperTests : IntegrationTestBase
	{
		[Test]
		public void Bootstrap_WhenAnalyzingAnAssemblyUsingAGivenType_ShouldResolveTypesFromThatAssembly()
		{
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Done();

			var result = kernel.Get<ITestingInterface>();

			result.GetType().Should().Be<TestingInterface>();
		}

		[Test]
		public void Bootstrap_WhenAnalyzingAnAssemblyUsingAGivenType_ShouldNotResolveOtherAssemblies()
		{
			Assert.Throws<ActivationException>(() =>
												   {
													   var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(ServiceAssemblyMarker) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Done();

													   var result = kernel.Get<ITestingInterface>();
												   });
		}

		[Test]
		public void Bootstrap_WhenBindingToStore_ShouldResovleIt()
		{
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Provider<TestDatabaseProvider>())
				.Done();

			var result = kernel.Get<IDocumentStore>();

			result.GetType().Should().Be<EmbeddableDocumentStore>();
		}

		[Test]
		public void Bootstrap_WhenBindingToConstantDatabase_ShouldResolveToThatConstant()
		{
			var store = new EmbeddableDocumentStore
							{
								RunInMemory = true
							};
			store.Initialize();

			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(store))
				.Done();

			var result = kernel.Get<IDocumentStore>();

			result.GetType().Should().Be<EmbeddableDocumentStore>();
		}

		[Test]
		public void Bootstrap_WhenBindingSettingsToStore_ShouldResolveSettingsUsingStore()
		{
			var entity = new
			{
				Id = "settings/Testing",
				JustSomeTextProperty = "from-store",
				JustSomeIntegerProperty = 10
			};

			Store(entity);


			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			var result = kernel.Get<ITestingSettings>();

			result.JustSomeTextProperty.Should().Be("from-store");
			result.JustSomeIntegerProperty.Should().Be(10);

		}

		[Test]
		public void Bootstrap_WhenSettingsDoesntExistInStore_ShouldReturnDefaultValues()
		{
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			var result = kernel.Get<ITestingSettings>();

			result.JustSomeTextProperty.Should().Be("default");
			result.JustSomeIntegerProperty.Should().Be(10);
		}

		[Test]
		public void Bootstrap_WhenSettingsExistInStoreButThePropertyDoesnt_ShouldReturnDefaultValues()
		{
			var entity = new
			{
				Id = "settings/Testing",
			};
			Store(entity);

			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			var result = kernel.Get<ITestingSettings>();

			result.JustSomeTextProperty.Should().Be("default");
		}

		[Test]
		public void Bootstrap_WhenBindingSettingsToJsonFiles_ShouldResolveSettingsUsingFile()
		{
			var entity = new
			{
				JustSomeTextProperty = "from-json",
				JustSomeIntegerProperty = 10
			};

			CreateJsonSettingsFile(entity);

			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();

			var result = kernel.Get<ITestingSettings>();

			result.JustSomeTextProperty.Should().Be("from-json");
			result.JustSomeIntegerProperty.Should().Be(10);

		}

		[Test]
		public void Bootstrap_WhenSettingsDoesntExistInJsonFiles_ShouldReturnDefaultValues()
		{
			DeleteJsonFile();

			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();

			var result = kernel.Get<ITestingSettings>();

			result.JustSomeTextProperty.Should().Be("default");
			result.JustSomeIntegerProperty.Should().Be(10);
		}

		[Test]
		public void Bootstrap_WhenSettingsExistInJsonButThePropertyDoesnt_ShouldReturnDefaultValues()
		{
			var entity = new
			{
			};

			CreateJsonSettingsFile(entity);

			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();

			var result = kernel.Get<ITestingSettings>();

			result.JustSomeTextProperty.Should().Be("default");
		}

		[Test]
		public void Bootstrap_WhenGivenAModule_ShouldLoadIt()
		{
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Done();

			var result = kernel.Get<IProvidedByAProvider>();

			result.Should().NotBeNull();
		}

		[Test]
		public void Bootstrap_WhenResolvingWithSingeltonConfiguration_ShouldBeInSingeltonScope()
		{
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.Configure(x=> x.InSingletonScope())
				.NoDatabase()
				.Done();

			var firstResolve = kernel.Get<IShouldBeSingelton>();
			firstResolve.Increase();

			var secondResolve = kernel.Get<IShouldBeSingelton>();

			secondResolve.State.Should().Be(1);
		}

		[Test]
		public void Bootstrap_WhenResolvingWithDefaultConfiguration_ShouldBeInSingeltonScope()
		{
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.Configure(x => x.InSingletonScope())
				.NoDatabase()
				.Done();

			var firstResolve = kernel.Get<IShouldBeSingelton>();
			firstResolve.Increase();

			var secondResolve = kernel.Get<IShouldBeSingelton>();

			secondResolve.State.Should().Be(1);
		}

		[Test]
		public void Bootstrap_WhenResolvingWithTransiantScope_ShouldBeCreateNeeOnjectAtEachResolve()
		{
			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.Configure(x => x.InTransientScope())
				.NoDatabase()
				.Done();

			var firstResolve = kernel.Get<IShouldBeSingelton>();
			firstResolve.Increase();

			var secondResolve = kernel.Get<IShouldBeSingelton>();

			secondResolve.State.Should().Be(0);
		}

		private void CreateJsonSettingsFile(dynamic setting)
		{
			using (var writter = new StreamWriter("settings/Testing.settings"))
			{
				writter.WriteLine(JsonConvert.SerializeObject(setting));
			}
		}

		private void DeleteJsonFile()
		{
			File.Delete("settings/Testing.settings");
		}

	}

	public interface IShouldBeSingelton
	{
		void Increase();
		int State { get; set; }
	}

	public class ShouldBeSingelton : IShouldBeSingelton
	{
		public int State { get; set; }

		public void Increase()
		{
			State++;
		}
	}

	public class TestingModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IProvidedByAProvider>().ToProvider<TestProvider>();
		}
	}

	public class TestProvider:Provider<IProvidedByAProvider>
	{
		protected override IProvidedByAProvider CreateInstance(IContext context)
		{
			return new ClassProvidedByProvider();
		}
	}

	public interface IProvidedByAProvider
	{
		
	}

	public class ClassProvidedByProvider:IProvidedByAProvider
	{
		
	}


	public interface ITestingSettings
	{
		[Default("default")]
		string JustSomeTextProperty { get; set; }
		[Default(10)]
		int JustSomeIntegerProperty { get; set; }
	}

	public class TestAssemblyMarkerType
	{ }

	public interface ITestingInterface
	{ }

	public class TestingInterface : ITestingInterface
	{ }

	public class TestDatabaseProvider : Provider<IDocumentStore>
	{
		protected override IDocumentStore CreateInstance(IContext context)
		{
			var store = new EmbeddableDocumentStore
							{
								RunInMemory = true
							};
			return store.Initialize();

		}
	}
}
