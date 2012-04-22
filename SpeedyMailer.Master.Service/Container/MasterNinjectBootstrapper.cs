using Ninject;
using SpeedyMailer.Core.Container;

namespace SpeedyMailer.Master.Service.Container
{
    public class MasterNinjectBootstrapper
    {
        public void Register(IKernel container)
        {
            container.BindInterfaces(x => x.FromAssembliesMatching(new[] {"SpeedyMailer.Core"}))
                .BindStoreTo<RavenDocumentStoreProvider>()
                .BindSettingsToDocumentStoreFor(x => x.FromAssembliesMatching(new[] {"SpeedyMailer.Core"}));
        }
    }
}
