using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Syntax;
using Raven.Client;

namespace SpeedyMailer.Core.Container
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
