using System;
using Castle.DynamicProxy;
using Raven.Client;

namespace SpeedyMailer.Core.Container
{
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
}