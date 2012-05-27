using System;
using Castle.DynamicProxy;
using Raven.Abstractions.Linq;

namespace SpeedyMailer.Core.Container
{
	public class SettingsInterceptor : SettingsInterceptorBase
	{
		public SettingsInterceptor(object settings, Type settingsInterface)
			: base(settings, settingsInterface)
		{ }

		protected override dynamic PersistantSetting(IInvocation invocation, Type returnType)
		{
			var name = ToAutoPropertyName(invocation);
			if (Settings != null)
			{
				var settingProperty = Settings.GetType().GetProperty(name);
				return settingProperty != null ? settingProperty.GetValue(Settings, null) : null;
			}
			return null;
		}
	}
}