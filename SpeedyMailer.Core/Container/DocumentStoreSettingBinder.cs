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
    public abstract class SettingBinderBase
    {
        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type type, IBindingRoot bindingRoot)
        {

            var settingsName = SettingsPresistanceName(type);

            var settings = ReadPresistantSettings(settingsName);

            var proxy = CreateProxy(type, settings);

            return new[] {bindingRoot.Bind(type).ToConstant<object>(proxy)};
        }

        private static string SettingsPresistanceName(Type type)
        {
            var settingsName = Regex.Match(type.Name, "I(.+?)Settings").Groups[1].Value;
            return settingsName;
        }

        protected abstract object ReadPresistantSettings(string settingsName);

        private static object CreateProxy(Type type, object settings)
        {
            var proxyGenerator = new ProxyGenerator();
            IInterceptor settingsInterceptor = new DocumentStoreSettingBinder.SettingsInterceptor(settings as DynamicJsonObject, type);
            var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(type, settingsInterceptor);
            return proxy;
        }
    }

    public class DocumentStoreSettingBinder : SettingBinderBase, IBindingGenerator
    {
        private readonly IDocumentStore _store;

        public DocumentStoreSettingBinder(IDocumentStore store)
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



        public class SettingsInterceptor : IInterceptor
        {
            private readonly DynamicJsonObject _settings;
            private readonly Type _settingsInterface;

            public SettingsInterceptor(DynamicJsonObject settings, Type settingsInterface)
            {
                _settings = settings;
                _settingsInterface = settingsInterface;
            }


            public void Intercept(IInvocation invocation)
            {
                var defaultAttr =
                    Attribute.GetCustomAttribute(_settingsInterface.GetProperty(ToAutoPropertyName(invocation)),
                                                 typeof (DefaultAttribute)) as DefaultAttribute;
                dynamic persistantSetting = PersistantSetting(invocation);

                if (_settings != null)
                {
                    if (persistantSetting != null)
                    {
                        invocation.ReturnValue = persistantSetting;
                    }
                    else
                    {
                        if (defaultAttr != null)
                        {
                            invocation.ReturnValue = defaultAttr.Text;
                        }
                    }
                }
                else
                {
                    invocation.ReturnValue = defaultAttr != null ? defaultAttr.Text : null;
                }
            }


            private static string ToAutoPropertyName(IInvocation invocation)
            {
                return invocation.Method.Name.Substring(4);
            }

            private dynamic PersistantSetting(IInvocation invocation)
            {
                string name = ToAutoPropertyName(invocation);
                return _settings != null ? _settings.GetValue(name) : null;
            }
        }

    }
}