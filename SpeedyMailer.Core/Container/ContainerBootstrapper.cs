using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;

namespace SpeedyMailer.Core.Container
{
    public class ContainerBootstrapper
    {
        private readonly ContainerStrapperOptions _localOptions;

        public ContainerBootstrapper(Action<ContainerStrapperOptions> options)
        {
            _localOptions = new ContainerStrapperOptions();
            options.Invoke(_localOptions);
        }

        public static AssemblyGatherer Bootstrap(IKernel kernel)
        {
            return new AssemblyGatherer(kernel);
        }
    }

    public class AssemblyGatherer
    {
        private readonly IKernel _kernel;
        public AssemblyGatherer(IKernel kernel)
        {
            _kernel = kernel;
        }

        public DatabaseBindingGatherer Analyze(Action<AssemblyGathererOptions> gatherAction)
        {
            var options = new AssemblyGathererOptions(_kernel);
            gatherAction.Invoke(options);
            
            return new DatabaseBindingGatherer(_kernel);
        }

       
    }

    public class DatabaseBindingGatherer
    {
        private readonly IKernel _kernel;

        public DatabaseBindingGatherer(IKernel kernel)
        {
            _kernel = kernel;
        }

        public SettingsResolverGatherer Storage(Action<DatabaseBindingGathererOptions> storageAction)
        {
            var options = new DatabaseBindingGathererOptions(_kernel);
            storageAction.Invoke(options);
            return new SettingsResolverGatherer(_kernel);
        }


    }

    public class DatabaseBindingGathererOptions
    {
        private readonly IKernel _kernel;

        public DatabaseBindingGathererOptions(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void Provider<T>()
        {
            
        }

        public void Constant<T>(T constant)
        {
            
        }

        public void NoDatabase()
        {
            
        }


    }

    public class SettingsResolverGatherer
    {
        private readonly IKernel _kernel;

        public SettingsResolverGatherer(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void Settings(Action<SettingsResolverGathererOptions> settingsAction)
        {
            
        }

        
    }

    public class SettingsResolverGathererOptions
    {
        private readonly IKernel _kernel;

        public SettingsResolverGathererOptions(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void UseDocumentDatabase()
        {

        }

        public void UseJsonFiles()
        {

        }
    }

    public class AssemblyGathererOptions
    {
        private readonly IKernel _kernel;

        public AssemblyGathererOptions(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void AssembiesContaining(Type[] types)
        {
        }
    }

    public class ContainerStrapperOptions
    {
        public SettingsResolvingType SettingsResolvingType { get; set; }
    }

    public enum SettingsResolvingType
    {
        Database=0,
        Json=1
    }

    public class ServiceAssembly
    {
    }

    public class UiAssembly
    {
    }

    public class CoreAssembly
    {
    }
}
