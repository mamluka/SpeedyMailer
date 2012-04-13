using Bootstrap.Ninject;
using Ninject;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Master.Service.Core.Container
{
    public class MasterNinjectBootstrapper : INinjectRegistration
    {
        public void Register(IKernel container)
        {
            container.BindInterfaces(x => x.FromAssembliesMatching(new[] {"SpeedyMailer.*"}))
                .BindStoreTo<RavenDocumentStoreProvider>()
                .BindSettingsToDocumentStoreFor(x => x.FromAssembliesMatching(new[] {"SpeedyMailer.*"}));
        }
    }
}
