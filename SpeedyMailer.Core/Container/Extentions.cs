using System;
using System.Diagnostics;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.BindingBuilder;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Extensions.Conventions.Syntax;
using Raven.Client;

namespace SpeedyMailer.Core.Container
{
    public static class Extentions
    {
        public static IKernel BindStoreTo<T>(this IKernel kernel) where T : IProvider
        {
            kernel.Bind<IDocumentStore>().ToProvider<T>();
            return kernel;
        }

        public static IKernel BindSettingsToDocumentStoreFor(this IKernel kernel, Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromAssemblies)
        {
            kernel.Bind(
                x =>
                fromAssemblies(x).Select(type => type.Name.EndsWith("Settings")).BindWith(kernel.Get<DocumentStoreSettingsBinder>()));
            return kernel;
        }

        public static IKernel BindInterfaces(this IKernel kernel,Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromAssemblies)
        {
            kernel.Bind(x => fromAssemblies(x).Select(type => !type.Name.EndsWith("Settings")).BindDefaultInterface());
            return kernel;
        }

        public static IKernel BindSettingsToJsonFilesFor(this IKernel kernel,Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromAssemblies)
        {
            kernel.Bind(
                x =>
                fromAssemblies(x).Select(type => type.Name.EndsWith("Settings")).BindWith(kernel.Get<JsonFileSettingsBinder>()));
            return kernel;
        }
    }
}