using System;
using System.IO;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpeedyMailer.Core.Container
{
    public class JsonFileSettingsBinder : SettingsBinderBase
    {
        protected override object ReadPresistantSettings(string settingsName)
        {
            var filename = string.Format("settings/{0}.settings", settingsName);
            if (!File.Exists(filename)) return null;

            using (var reader = new StreamReader(filename))
            {
                return JsonConvert.DeserializeObject<object>(reader.ReadToEnd());
            }
        }

        protected override IInterceptor SetInterceptor(Type type, object settings)
        {
            return new JsonSettingsInterceptor(settings as JObject,type);
        }
    }
}