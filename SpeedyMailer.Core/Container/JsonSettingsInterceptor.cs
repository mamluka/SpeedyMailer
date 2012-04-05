using System;
using Castle.DynamicProxy;
using Newtonsoft.Json.Linq;

namespace SpeedyMailer.Core.Container
{
    public class JsonSettingsInterceptor:SettingsInterceptorBase
    {
        public JsonSettingsInterceptor(object settings, Type settingsInterface) : base(settings, settingsInterface)
        {
        }

        protected override dynamic PersistantSetting(IInvocation invocation)
        {
            var name = ToAutoPropertyName(invocation);
            return Settings != null ? (Settings as JObject)[name] != null ? (Settings as JObject)[name].ToObject<string>(): null : null;
        }
    }
}