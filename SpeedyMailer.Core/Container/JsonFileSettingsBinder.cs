using System;
using System.IO;
using Castle.DynamicProxy;
using Newtonsoft.Json;

namespace SpeedyMailer.Core.Container
{
    public class JsonFileSettingsBinder : SettingsBinderBase
    {
        protected override object ReadPresistantSettings(string settingsName)
        {
            var reader = new StreamReader(string.Format("settings/{0}.settings", settingsName));
            return JsonConvert.DeserializeObject<object>(reader.ReadToEnd());
        }

        protected override IInterceptor SetInterceptor(Type type, object settings)
        {
            return new JsonSettingsInterceptor(settings,type);
        }
    }
}