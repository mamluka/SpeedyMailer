using System;
using System.Collections.Generic;
using Ninject;
using Ninject.Extensions.Conventions.Syntax;

namespace SpeedyMailer.Core.Container
{
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
}