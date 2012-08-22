using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Activation;
#if MONO
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Mono.Extensions.Conventions.Syntax;
#else
using Ninject.Extensions.Conventions.Syntax;
#endif
using Ninject.Syntax;
using Raven.Client;
using SpeedyMailer.Core.Container;
#if MONO
using Ninject.Mono.Extensions.Conventions;
#else
using Ninject.Extensions.Conventions;
#endif

namespace SpeedyMailer.Master.Service.Container
{
	public class ContainerBootstrapper
	{
		public static IKernel Kernel;

		private static ContainerStrapperOptions currentOptions;

		public static AssemblyGatherer Bootstrap(IKernel kernel = null)
		{
			currentOptions = new ContainerStrapperOptions();
			Kernel = kernel;
			return new AssemblyGatherer(currentOptions);
		}

		public static IKernel Execute(ContainerStrapperOptions containerStrapperOptions, IKernel kernel)
		{
			currentOptions = containerStrapperOptions;
			return ExecuteOptions(kernel);
		}

		public static IKernel Execute(ContainerStrapperOptions containerStrapperOptions)
		{
			var kernel = new StandardKernel();
			currentOptions = containerStrapperOptions;
			return ExecuteOptions(kernel);
		}

		private static IKernel ExecuteOptions(IKernel kernel)
		{
			var bindFunction = BindFunction();
			var fromFunction = FromFunction();
			var selectFunction = SelectFunction();

			kernel.Bind(x => ApplyBindFunction(x, fromFunction, bindFunction, selectFunction).Configure(currentOptions.ConfigurationAction));
			kernel.Load(GetAssembliesToAnalyze());

			BindDatabase(kernel);
			BindSettings(kernel);

			return kernel;
		}

		private static void BindSettings(IKernel kernel)
		{
			if (currentOptions.Settings == SettingsType.DocumentDatabase)
			{
				try
				{
					var store = kernel.Get<IDocumentStore>();

					BindStoreSettings(kernel, store);
				}
				catch (ActivationException)
				{
					throw new ContainerException(typeof(IDocumentStore),
												 "When resolving the document store for settings binder no canidate were found");
				}
			}

			if (currentOptions.Settings == SettingsType.JsonFiles)
			{
				BindJsonSettings(kernel);
			}
		}

		private static void BindJsonSettings(IKernel kernel)
		{
			kernel.Bind(x => SelectSettingsTypes(x).BindWith(
				new JsonFileSettingsBinder())
				);
		}

		private static void BindStoreSettings(IKernel kernel, IDocumentStore store)
		{
			kernel.Bind(x => SelectSettingsTypes(x).BindWith(
				new DocumentStoreSettingsBinder(store))
				);
		}

		private static void BindDatabase(IKernel kernel)
		{
			if (currentOptions.DatabaseBindingFunction != null)
			{
				currentOptions.DatabaseBindingFunction(kernel);
			}
		}

		private static IConfigureSyntax ApplyBindFunction(IFromSyntax x, Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromFunction, Func<IJoinExcludeIncludeBindSyntax, IConfigureSyntax> bindFunction, Func<IIncludingNonePublicTypesSelectSyntax, IJoinExcludeIncludeBindSyntax> selectFunction)
		{
			return bindFunction(selectFunction(fromFunction(x)));
		}

		private static Func<IIncludingNonePublicTypesSelectSyntax, IJoinExcludeIncludeBindSyntax> SelectFunction()
		{
			Func<IIncludingNonePublicTypesSelectSyntax, IJoinExcludeIncludeBindSyntax> selectFunction =
				selectSyntax => selectSyntax.SelectAllClasses();

			if (currentOptions.SelectingStrategy == SelectingStrategy.All)
			{
				selectFunction = selectSyntax => selectSyntax.SelectAllTypes();
			}
			return selectFunction;
		}

		private static Func<IJoinExcludeIncludeBindSyntax, IConfigureSyntax> BindFunction()
		{
			Func<IJoinExcludeIncludeBindSyntax, IConfigureSyntax> bindFunction = bindSyntax => bindSyntax.BindDefaultInterface();

			if (currentOptions.BindingStratery == BindingStrategy.BindInterfaceToDefaultImplementation)
			{
				bindFunction = bindSyntax => bindSyntax.BindDefaultInterface();
			}
			return bindFunction;
		}

		private static Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> FromFunction()
		{
			Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromFunction = fromSyntax => fromSyntax.FromThisAssembly();
			if (currentOptions.AnalyzeStrategy == AnalyzeStrategy.ByTypes)
			{
				fromFunction = fromSyntax => fromSyntax.From(GetAssembliesToAnalyze());
			}
			return fromFunction;
		}

		private static IEnumerable<Assembly> GetAssembliesToAnalyze()
		{
			return currentOptions.TypesToAnalyze.Select(type => type.Assembly);
		}

		private static IJoinExcludeIncludeBindSyntax SelectSettingsTypes(IFromSyntax x)
		{
			var fromFunction = FromFunction();
			return fromFunction(x).Select(SettingsNameConventionFilter);
		}

		private static bool SettingsNameConventionFilter(Type type)
		{
			return type.Name.EndsWith("Settings");
		}

		public static void ReloadStoreSetting<T>(IKernel kernel, IDocumentStore documentStore)
		{
			kernel.Unbind<T>();
			kernel.Bind(
				from => from
					.FromAssemblyContaining<T>()
					.Select(x => x == typeof(T))
					.BindWith(new DocumentStoreSettingsBinder(documentStore))
				);
		}

		public static void ReloadJsonSetting<T>(IKernel kernel, string settingsFolder = "settings")
		{
			kernel.Unbind<T>();
			kernel.Bind(
				from => from
					.FromAssemblyContaining<T>()
					.Select(x => x == typeof(T))
					.BindWith(new JsonFileSettingsBinder(settingsFolder))
				);
		}

		public static void ReloadAllStoreSettings(IKernel kernel, IDocumentStore documentStore = null)
		{
			if (documentStore == null)
				documentStore = kernel.Get<IDocumentStore>();

			UnbindAllSettings(kernel);
			BindStoreSettings(kernel, documentStore);
		}

		private static void UnbindAllSettings(IBindingRoot kernel)
		{
			foreach (var assembly in GetAssembliesToAnalyze())
			{
				assembly
					.GetTypes()
					.Where(SettingsNameConventionFilter)
					.ToList()
					.ForEach(kernel.Unbind);
			}
		}

		public static void ReloadAllJsonSetting(IKernel kernel)
		{
			UnbindAllSettings(kernel);
			BindJsonSettings(kernel);
		}
	}

	public class ContainerException : Exception
	{
		private string _message;
		private Type _type;

		public ContainerException(Type type, string message)
		{
			_type = type;
			_message = message;
		}
	}

	public class ContainerStrapperOptions
	{
		public BindingStrategy BindingStratery { get; set; }
		public AnalyzeStrategy AnalyzeStrategy { get; set; }
		public IEnumerable<Type> TypesToAnalyze { get; set; }
		public SettingsType Settings { get; set; }
		public SelectingStrategy SelectingStrategy { get; set; }
		public Action<IKernel> DatabaseBindingFunction { get; set; }
		public Action<IKernel> SettingsResolverFunction { get; set; }
		public bool NoDatabase { get; set; }
		public ConfigurationAction ConfigurationAction { get; set; }
	}

	public interface IDoneBootstrapping
	{
		IKernel Done();
	}

	public class DoneBootstrapping : IDoneBootstrapping
	{
		private readonly ContainerStrapperOptions _options;

		public DoneBootstrapping(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public IKernel Done()
		{
			if (ContainerBootstrapper.Kernel != null)
			{
				return ContainerBootstrapper.Execute(_options, ContainerBootstrapper.Kernel);
			}
			return ContainerBootstrapper.Execute(_options);
		}
	}

	public class DatabaseBindingGatherer : IDoneBootstrapping
	{
		private readonly ContainerStrapperOptions _options;

		public DatabaseBindingGatherer(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public SettingsResolverGatherer Storage<T>(Action<DatabaseBindingGathererOptions<T>> storageAction)
		{
			var databaseBindingGathererOptions = new DatabaseBindingGathererOptions<T>(_options);
			storageAction.Invoke(databaseBindingGathererOptions);
			return new SettingsResolverGatherer(_options);
		}

		public IKernel Done()
		{
			return ContainerBootstrapper.Execute(_options);
		}


		public SettingsResolverGatherer NoDatabase()
		{
			_options.NoDatabase = true;
			return new SettingsResolverGatherer(_options);
		}
	}

	public class SettingsResolverGatherer : IDoneBootstrapping
	{
		private readonly ContainerStrapperOptions _options;

		public SettingsResolverGatherer(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public IDoneBootstrapping Settings(Action<SettingsResolverGathererOptions> settingsAction)
		{
			var settingsResolverGathererOptions = new SettingsResolverGathererOptions(_options);
			settingsAction.Invoke(settingsResolverGathererOptions);
			return new DoneBootstrapping(_options);
		}

		public IKernel Done()
		{
			return ContainerBootstrapper.Execute(_options);
		}
	}

	public class AssemblyGatherer
	{
		private readonly ContainerStrapperOptions _options;

		public AssemblyGatherer(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public BindingStrategyGatherer Analyze(Action<AssemblyGathererOptions> gatherAction)
		{
			var assemblyGathererOptions = new AssemblyGathererOptions(_options);
			gatherAction.Invoke(assemblyGathererOptions);
			return new BindingStrategyGatherer(_options);
		}


	}

	public class ConfigurationsGatherer
	{
		private readonly ContainerStrapperOptions _options;

		public ConfigurationsGatherer(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public DatabaseBindingGatherer Configure(ConfigurationAction action)
		{
			_options.ConfigurationAction = action;
			return new DatabaseBindingGatherer(_options);
		}

		public DatabaseBindingGatherer DefaultConfiguration()
		{
			_options.ConfigurationAction = opt => opt.InSingletonScope();
			return new DatabaseBindingGatherer(_options);
		}
	}

	public class AssemblyGathererOptions
	{
		private readonly ContainerStrapperOptions _options;

		public AssemblyGathererOptions(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public void AssembiesContaining(IEnumerable<Type> types)
		{
			_options.AnalyzeStrategy = AnalyzeStrategy.ByTypes;
			_options.TypesToAnalyze = types;
		}
	}

	public class BindingStrategyGatherer
	{
		private readonly ContainerStrapperOptions _options;

		public BindingStrategyGatherer(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public ConfigurationsGatherer BindInterfaceToDefaultImplementation()
		{
			_options.BindingStratery = BindingStrategy.BindInterfaceToDefaultImplementation;
			return new ConfigurationsGatherer(_options);
		}


	}

	public class DatabaseBindingGathererOptions<T>
	{
		private readonly ContainerStrapperOptions _options;

		public DatabaseBindingGathererOptions(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public void Provider<TProvider>() where TProvider : IProvider
		{
			_options.DatabaseBindingFunction = kernel => kernel.Bind<T>().ToProvider<TProvider>();
		}

		public void Constant(T constant)
		{
			_options.DatabaseBindingFunction = kernel => kernel.Bind<T>().ToConstant(constant);
		}

		public void NoDatabase()
		{
			_options.DatabaseBindingFunction = kernel => kernel.Bind<IDocumentStore>().ToConstant(new NoRavenSupport());
		}
	}

	public class SettingsResolverGathererOptions
	{
		private readonly ContainerStrapperOptions _options;

		public SettingsResolverGathererOptions(ContainerStrapperOptions options)
		{
			_options = options;
		}

		public void UseDocumentDatabase()
		{
			_options.Settings = SettingsType.DocumentDatabase;
		}

		public void UseJsonFiles()
		{
			_options.Settings = SettingsType.JsonFiles;
		}
	}

	public enum SelectingStrategy
	{
		All
	}

	public enum SettingsType
	{
		None,
		DocumentDatabase,
		JsonFiles
	}
}
