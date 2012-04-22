using System;
using Castle.DynamicProxy;
using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Core.Container
{
    public abstract class SettingsInterceptorBase : IInterceptor
    {
        protected readonly object Settings;
        protected readonly Type SettingsInterface;


        protected SettingsInterceptorBase(object settings, Type settingsInterface)
        {
            Settings = settings;
            SettingsInterface = settingsInterface;
        }

        public void Intercept(IInvocation invocation)
        {
            var defaultAttr =
                Attribute.GetCustomAttribute(SettingsInterface.GetProperty(ToAutoPropertyName(invocation)),
                                             typeof (DefaultAttribute)) as DefaultAttribute;
            dynamic persistantSetting = PersistantSetting(invocation);

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
                        invocation.ReturnValue = defaultAttr.Text;
                    }
                }
            }
            else
            {
                invocation.ReturnValue = defaultAttr != null ? defaultAttr.Text : null;
            }
        }

        protected static string ToAutoPropertyName(IInvocation invocation)
        {
            return invocation.Method.Name.Substring(4);
        }

        protected abstract dynamic PersistantSetting(IInvocation invocation);
    }
}