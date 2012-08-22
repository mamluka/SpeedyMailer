using System.IO;
using NUnit.Framework;
using Newtonsoft.Json;
using Ninject;
using Ninject.Activation;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Embedded;
using SpeedyMailer.Core.Container;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Master.Service;
using FluentAssertions;
using SpeedyMailer.Master.Service.Container;
using SpeedyMailer.Tests.Core.Integration.Base;

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
			var entity = new TestingSettings
			{
				JustSomeTextProperty = "from-store",
				JustSomeIntegerProperty = 10
			};

			Store(entity,"settings/Testing");


			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			var result = kernel.Get<TestingSettings>();

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

			var result = kernel.Get<TestingSettings>();

			result.JustSomeTextProperty.Should().Be("default");
			result.JustSomeIntegerProperty.Should().Be(10);
		}

		[Test]
		public void Bootstrap_WhenSettingsExistInStoreButThePropertyDoesnt_ShouldReturnDefaultValues()
		{
			var entity = new TestingSettings();
			Store(entity);

			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			var result = kernel.Get<TestingSettings>();

			result.JustSomeTextProperty.Should().Be("default");
		}

		[Test]
		public void Bootstrap_WhenBindingSettingsToJsonFiles_ShouldResolveSettingsUsingFile()
		{
			var entity = new TestingSettings
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

			var result = kernel.Get<TestingSettings>();

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

			var result = kernel.Get<TestingSettings>();

			result.JustSomeTextProperty.Should().Be("default");
			result.JustSomeIntegerProperty.Should().Be(10);
		}

		[Test]
		public void Bootstrap_WhenSettingsExistInJsonButThePropertyDoesnt_ShouldReturnDefaultValues()
		{
			var entity = new TestingSettings();

			CreateJsonSettingsFile(entity);

			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.NoDatabase()
				.Settings(x => x.UseJsonFiles())
				.Done();

			var result = kernel.Get<TestingSettings>();

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

		[Test]
		public void ReloadStoreSetting_WhenCalled_ShouldReloadTheSpecifiedSetting()
		{
			var entity = new TestingSettings
			{
				JustSomeTextProperty = "from-store",
				JustSomeIntegerProperty = 10
			};

			Store(entity, "settings/Testing");


			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			entity.JustSomeTextProperty = "reload-me";
			Store(entity, "settings/Testing");

			ContainerBootstrapper.ReloadStoreSetting<TestingSettings>(kernel, DocumentStore);

			var setting = kernel.Get<TestingSettings>();

			setting.JustSomeTextProperty.Should().Be("reload-me");
		}
		
		[Test]
		public void ReloadAllStoreSettings_WhenCalled_ShouldReloadTheSpecifiedSetting()
		{
			var entity = new TestingSettings
			{
				JustSomeTextProperty = "from-store",
				JustSomeIntegerProperty = 10
			};

			Store(entity, "settings/Testing");


			var kernel = ContainerBootstrapper
				.Bootstrap()
				.Analyze(x => x.AssembiesContaining(new[] { typeof(TestAssemblyMarkerType) }))
				.BindInterfaceToDefaultImplementation()
				.DefaultConfiguration()
				.Storage<IDocumentStore>(x => x.Constant(DocumentStore))
				.Settings(x => x.UseDocumentDatabase())
				.Done();

			entity.JustSomeTextProperty = "reload-me";
			Store(entity, "settings/Testing");

			ContainerBootstrapper.ReloadAllStoreSettings(kernel);

			var setting = kernel.Get<TestingSettings>();

			setting.JustSomeTextProperty.Should().Be("reload-me");
		}

		[Test]
		public void ReloadJsonSetting_WhenCalled_ShouldReloadTheSpecifiedSetting()
		{
			var entity = new TestingSettings
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


			entity.JustSomeTextProperty = "reload-me";
			CreateJsonSettingsFile(entity);

			ContainerBootstrapper.ReloadJsonSetting<TestingSettings>(kernel);

			var setting = kernel.Get<TestingSettings>();

			setting.JustSomeTextProperty.Should().Be("reload-me");
		}
		
		public void ReloadAllJsonSetting_WhenCalled_ShouldReloadTheSpecifiedSetting()
		{
			var entity = new TestingSettings
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


			entity.JustSomeTextProperty = "reload-me";
			CreateJsonSettingsFile(entity);

			ContainerBootstrapper.ReloadAllJsonSetting(kernel);

			var setting = kernel.Get<TestingSettings>();

			setting.JustSomeTextProperty.Should().Be("reload-me");
		}

		private void CreateJsonSettingsFile(dynamic setting)
		{
			if (!Directory.Exists("settings"))
				Directory.CreateDirectory("settings");

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


	public class TestingSettings
	{
		[Default("default")]
		public virtual string JustSomeTextProperty { get; set; }
		[Default(10)]
		public virtual int JustSomeIntegerProperty { get; set; }
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
