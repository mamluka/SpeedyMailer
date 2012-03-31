using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using ImpromptuInterface;
using ImpromptuInterface.Dynamic;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.Syntax;
using Raven.Client;

namespace SpeedyMailer.Core.Container
{
    public static class Extentions
    {
        public static IKernel BindStoreTo<T>(this IKernel kernel) where T:IProvider
        {
            kernel.Bind<IDocumentStore>().ToProvider<T>();
            return kernel;
        }

        public static IKernel BindSettingsFor(this IKernel kernel, Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromAssemblies)
        {
            kernel.Bind(x => fromAssemblies(x).SelectAllInterfaces().BindWith(kernel.Get<SettingsBinder>()));
            return kernel;
        }
    }

    

    public class DefaultAttribute : Attribute
    {
        public string Text { get; set; }

        public DefaultAttribute(string david)
        {
            Text = david;
        }
    }
}
