using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Castle.DynamicProxy;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Syntax;

namespace SpeedyMailer.Core.Container
{
	public abstract class SettingsBinderBase : IBindingGenerator
	{
		public IEnumerable<IBindingWhenInNamedWithOrOnSyntax<object>> CreateBindings(Type type, IBindingRoot bindingRoot)
		{

			var settingsName = SettingsPresistanceName(type);

			var settings = ReadPresistantSettings(settingsName);

			var proxy = CreateProxy(type, settings);

			return new[] { bindingRoot.Bind(type).ToConstant(proxy) };
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
}