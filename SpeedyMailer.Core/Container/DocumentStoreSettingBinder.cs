using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ninject.Extensions.Conventions.BindingGenerators;
using Ninject.Syntax;
using Raven.Client;

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
			var settingsName = Regex.Match(type.Name, "(.+?)Settings").Groups[1].Value;
			return settingsName;
		}

		protected abstract object ReadPresistantSettings(string settingsName);
		protected abstract IInterceptor SetInterceptor(Type type, object settings);

		private object CreateProxy(Type type, object settings)
		{
			var proxyGenerator = new ProxyGenerator();
			var settingsInterceptor = SetInterceptor(type, settings);
			var proxy = proxyGenerator.CreateClassProxy(type, settingsInterceptor);
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
			{ }
			return settings;
		}

		protected override IInterceptor SetInterceptor(Type type, object settings)
		{
			return new SettingsInterceptor(settings, type);
		}
	}

	public class JsonFileSettingsBinder : SettingsBinderBase
	{
		private readonly string _settingFoldername;

		public JsonFileSettingsBinder(string settingFoldername = "settings")
		{
			_settingFoldername = settingFoldername;
		}

		protected override object ReadPresistantSettings(string settingsName)
		{
			var filename = string.Format("{0}/{1}.settings", _settingFoldername, settingsName);
			if (!File.Exists(filename)) return null;

			using (var reader = new StreamReader(filename))
			{
				return JsonConvert.DeserializeObject<object>(reader.ReadToEnd());
			}
		}

		protected override IInterceptor SetInterceptor(Type type, object settings)
		{
			return new JsonSettingsInterceptor(settings as JObject, type);
		}
	}

	public class JsonSettingsInterceptor : SettingsInterceptorBase
	{
		public JsonSettingsInterceptor(object settings, Type settingsInterface)
			: base(settings, settingsInterface)
		{
		}

		protected override dynamic PersistantSetting(IInvocation invocation, Type returnType)
		{
			if (Settings == null) return null;

			var name = ToAutoPropertyName(invocation);
			var jToken = (Settings as JObject)[name];

			if (jToken == null) return null;

			var method = jToken.GetType().GetMethod("ToObject", new Type[] { });
			var generic = method.MakeGenericMethod(returnType);

			var persistantSetting = generic.Invoke(jToken, null);

			return jToken != null ? persistantSetting : null;
		}
	}
}