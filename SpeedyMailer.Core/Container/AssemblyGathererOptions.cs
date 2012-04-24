using System;
using System.Collections.Generic;
using Ninject;

namespace SpeedyMailer.Core.Container
{
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
}