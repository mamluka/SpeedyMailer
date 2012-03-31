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
    public class SettingsBinder : IBindingGenerator
    {
        private readonly IDocumentStore _store;

        public SettingsBinder(IDocumentStore store)
        {
            _store = store;
        }

        public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type type, IBindingRoot bindingRoot)
        {
            var proxyGenerator = new ProxyGenerator();
            object settings= null;

            var settingsName = Regex.Match(type.Name, "I(.+?)Settings").Groups[1].Value;
            var settingsId = string.Format("settings/{0}", settingsName);
            try
            {
                using (var session = _store.OpenSession())
                {
                    settings = session.Load<object>(settingsId);
                }
            }
            catch{}

            IInterceptor settingsInterceptor = new SettingsInterceptor(settings as DynamicJsonObject, type);
            var proxy = proxyGenerator.CreateInterfaceProxyWithoutTarget(type, settingsInterceptor);

            return new[] {bindingRoot.Bind(type).ToConstant(proxy)};
        }

        public class SettingsInterceptor : IInterceptor
        {
            private readonly DynamicJsonObject _settings;
            private readonly Type _settingsInterface;

            public SettingsInterceptor(dynamic settings, Type settingsInterface)
            {
                _settings = settings;
                _settingsInterface = settingsInterface;
            }

            public void Intercept(IInvocation invocation)
            {

                var defaultAttr = Attribute.GetCustomAttribute(_settingsInterface.GetProperty(ToAutoPropertyName(invocation)), typeof (DefaultAttribute)) as DefaultAttribute;
                dynamic persistantSetting = PersistantSetting(invocation);

                if (_settings != null )
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
                var name = ToAutoPropertyName(invocation);
                return _settings != null ? _settings.GetValue(name) : null;
            }
        }
    }
}