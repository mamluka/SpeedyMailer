using System;
using System.Linq;
using System.Text;
using Bootstrap.Extensions.Containers;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.Syntax;
using Raven.Client;

namespace SpeedyMailer.Core.Container
{
    public class ContainerBootstrapper
    {
        private static readonly ContainerStrapperOptions Options = new ContainerStrapperOptions();
    	public static IKernel Kernel;

    	public static AssemblyGatherer Bootstrap(IKernel kernel = null)
    	{
    		Kernel = kernel;
    		return new AssemblyGatherer(Options);
    	}

    	public static IKernel Execute(ContainerStrapperOptions containerStrapperOptions,IKernel kernel)
        {
        	return ExecuteOptions(containerStrapperOptions, kernel);
        }

		public static IKernel Execute(ContainerStrapperOptions containerStrapperOptions)
		{
			var kernel = new StandardKernel();
			return ExecuteOptions(containerStrapperOptions, kernel);
		}

    	private static IKernel ExecuteOptions(ContainerStrapperOptions containerStrapperOptions, IKernel kernel)
    	{
    		Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromFunction = fromSyntax => fromSyntax.FromThisAssembly();
    		Func<IIncludingNonePublicTypesSelectSyntax, IJoinExcludeIncludeBindSyntax> selectFunction =
    			selectSyntax => selectSyntax.SelectAllClasses();
    		Func<IJoinExcludeIncludeBindSyntax, IConfigureSyntax> bindFunction = bindSyntax => bindSyntax.BindDefaultInterface();

    		if (containerStrapperOptions.BindingStratery == BindingStrategy.BindInterfaceToDefaultImplementation)
    		{
    			bindFunction = bindSyntax => bindSyntax.BindDefaultInterface();
    		}
    		if (containerStrapperOptions.SelectingStrategy == SelectingStrategy.All)
    		{
    			selectFunction = selectSyntax => selectSyntax.SelectAllTypes();
    		}

    		if (containerStrapperOptions.AnalyzeStrategy == AnalyzeStrategy.ByTypes)
    		{
    			fromFunction = fromSyntax => fromSyntax.From(containerStrapperOptions.TypesToAnalyze.Select(type => type.Assembly));
    		}

    		kernel.Bind(x => bindFunction(selectFunction(fromFunction(x))));

    		if (containerStrapperOptions.DatabaseBindingFunction != null)
    		{
    			Options.DatabaseBindingFunction(kernel);
    		}

    		if (containerStrapperOptions.Settings == SettingsType.DocumentDatabase)
    		{
    			try
    			{
    				var store = kernel.Get<IDocumentStore>();

    				kernel.Bind(x =>
    				            fromFunction(x).Select(type => type.Name.EndsWith("Settings") && type.IsInterface).BindWith(
    				            	new DocumentStoreSettingsBinder(store))
    					);
    			}
    			catch (ActivationException)
    			{
    				throw new ContainerException(typeof (IDocumentStore),
    				                             "When resolving the document store for settings binder no canidate was found");
    			}
    		}

    		if (containerStrapperOptions.Settings == SettingsType.JsonFiles)
    		{
    			kernel.Bind(x =>
    			            fromFunction(x).Select(type => type.Name.EndsWith("Settings") && type.IsInterface).BindWith(
    			            	new JsonFileSettingsBinder())
    				);
    		}

    		return kernel;
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
