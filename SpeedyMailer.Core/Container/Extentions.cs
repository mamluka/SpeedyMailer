using System;
using System.Collections.Generic;
using System.IO;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject;
using Ninject.Activation;
using Ninject.Extensions.Conventions;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Extensions.Conventions.Syntax;
using Ninject.Syntax;
using Raven.Abstractions.Linq;
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

        public static IKernel BindSettingsToDocumentStoreFor(this IKernel kernel,
                                              Func<IFromSyntax, IIncludingNonePublicTypesSelectSyntax> fromAssemblies)
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
                fromAssemblies(x).Select(type => type.Name.EndsWith("Settings")).BindWith(kernel.Get<JsonFileSettingBinder>()));
            return kernel;
        }

    }

    public class JsonFileSettingBinder : SettingsBinderBase
    {
        protected override object ReadPresistantSettings(string settingsName)
        {
            var reader = new StreamReader(string.Format("settings/{0}.settings", settingsName));
            return JsonConvert.DeserializeObject<object>(reader.ReadToEnd());
        }

        protected override IInterceptor SetInterceptor(Type type, object settings)
        {
            return new JsobInterceptor(settings,type);
        }
    }

    public class JsobInterceptor:SettingsInterceptorBase
    {
        public JsobInterceptor(object settings, Type settingsInterface) : base(settings, settingsInterface)
        {
        }

        protected override dynamic PersistantSetting(IInvocation invocation)
        {
            var name = ToAutoPropertyName(invocation);
            return Settings != null ? (Settings as JObject)[name].ToObject<string>() : null;
        }
    }
}