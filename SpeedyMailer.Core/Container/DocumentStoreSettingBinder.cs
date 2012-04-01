using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Castle.DynamicProxy;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Syntax;
using Raven.Abstractions.Linq;
using Raven.Client;

namespace SpeedyMailer.Core.Container
{
    public abstract class SettingsBinderBase:IBindingGenerator
    {
        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type type, IBindingRoot bindingRoot)
        {

            var settingsName = SettingsPresistanceName(type);

            var settings = ReadPresistantSettings(settingsName);

            var proxy = CreateProxy(type, settings);

            return new[] {bindingRoot.Bind(type).ToConstant(proxy)};
        }

        private string SettingsPresistanceName(Type type)
        {
            var settingsName = Regex.Match(type.Name, "I(.+?)Settings").Groups[1].Value;
            return settingsName;
        }

        protected abstract object ReadPresistantSettings(string settingsName);
        protected abstract IInterceptor SetInterceptor(Type type, object settings);

        private object CreateProxy(Type type, object settings)
        {
            var proxyGenerator = new ProxyGenerator();
            var settingsInterceptor = SetInterceptor(type, settings);
            var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(type, settingsInterceptor);
            return proxy;
        }
    }

    public class DocumentStoreSettingsBinder : SettingsBinderBase
    {
        private readonly IDocumentStore _store;

        public DocumentStoreSettingsBinder(IDocumentStore store)
        {
            _store = store;
        }


        protected override object ReadPresistantSettings(string settingsName)
        {
            object settings = null;
            var settingsId = string.Format("settings/{0}", settingsName);
            try
            {
                using (IDocumentSession session = _store.OpenSession())
                {
                    settings = session.Load<object>(settingsId);
                }
            }
            catch
            {
            }
            return settings;
        }

        protected override IInterceptor SetInterceptor(Type type, object settings)
        {
            IInterceptor settingsInterceptor = new SettingsInterceptor(settings as DynamicJsonObject, type);
            return settingsInterceptor;
        }
    }

    public class SettingsInterceptor : SettingsInterceptorBase
    {
        public SettingsInterceptor(object settings, Type settingsInterface) : base(settings, settingsInterface)
        {
        }

        protected override dynamic PersistantSetting(IInvocation invocation)
        {
            string name = ToAutoPropertyName(invocation);
            return Settings != null ? (Settings as DynamicJsonObject).GetValue(name) : null;
        }
    }
}