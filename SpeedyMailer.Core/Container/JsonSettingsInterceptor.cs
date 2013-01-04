using System;
using Castle.DynamicProxy;
using Newtonsoft.Json.Linq;
using Raven.Client.Util;

namespace SpeedyMailer.Core.Container
{
    public class JsonSettingsInterceptor:SettingsInterceptorBase
    {
        public JsonSettingsInterceptor(object settings, Type settingsInterface) : base(settings, settingsInterface)
        {
        }

        protected override dynamic PersistantSetting(IInvocation invocation, Type returnType)
        {
            if (Settings == null) return null;

            var name = ToAutoPropertyName(invocation);
            var jToken = (Settings as JObject)[name];

			if (jToken == null) return null;

            var method = jToken.GetType().GetMethod("ToObject",new Type[]{});
            var generic = method.MakeGenericMethod(returnType);

            var persistantSetting = generic.Invoke(jToken, null);

	        return persistantSetting;
        }
    }
}