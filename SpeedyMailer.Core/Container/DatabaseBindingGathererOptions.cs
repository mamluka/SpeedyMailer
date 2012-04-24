using Ninject;
using Ninject.Activation;

namespace SpeedyMailer.Core.Container
{
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
            
        }
    }
}