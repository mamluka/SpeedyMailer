using System;
using System.Collections.Generic;
using System.Diagnostics;
using Castle.DynamicProxy;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Core.Container
{
	public abstract class SettingsInterceptorBase : IInterceptor
	{
		protected readonly object Settings;
		protected readonly Type SettingsInterface;
		private readonly Dictionary<string, object> _storedSettings = new Dictionary<string, object>();


		protected SettingsInterceptorBase(object settings, Type settingsInterface)
		{
			Settings = settings;
			SettingsInterface = settingsInterface;
		}

		public void Intercept(IInvocation invocation)
		{
			var methodName = GetMethodName(invocation);

//			Trace.WriteLine("Settings method called:" + methodName);

			if (invocation.Arguments.Length > 0)
			{
				_storedSettings[methodName] = invocation.Arguments[0];
				return;
			}

			var defaultAttr =
				Attribute.GetCustomAttribute(SettingsInterface.GetProperty(ToAutoPropertyName(invocation)),
											 typeof(DefaultAttribute)) as DefaultAttribute;


			var persistantSetting = PersistantSetting(invocation, invocation.Method.ReturnType);

			if (Settings != null)
			{
				if (persistantSetting != null)
				{
					invocation.ReturnValue = persistantSetting;
				}
				else
				{
					if (defaultAttr != null)
					{
						invocation.ReturnValue = defaultAttr.Value;
					}
				}
			}
			else
			{
				invocation.ReturnValue = defaultAttr != null ? defaultAttr.Value : _storedSettings.ContainsKey(methodName) ? _storedSettings[methodName] : null;
			}
		}

		private static string GetMethodName(IInvocation invocation)
		{
			var methodName = invocation.Method.Name;
			if (methodName.StartsWith("set_") || methodName.StartsWith("get_"))
			{
				methodName = methodName.Substring(4);
			}
			return methodName;
		}

		protected static string ToAutoPropertyName(IInvocation invocation)
		{
			return invocation.Method.Name.Substring(4);
		}

		protected abstract dynamic PersistantSetting(IInvocation invocation, Type returnType);
	}
}