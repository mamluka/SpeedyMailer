using System;
using System.IO;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpeedyMailer.Core.Container
{
    public class JsonFileSettingsBinder : SettingsBinderBase
    {
    	private readonly string _settingFoldername;

    	public JsonFileSettingsBinder(string settingFoldername="settings")
    	{
    		_settingFoldername = settingFoldername;
    	}

    	protected override object ReadPresistantSettings(string settingsName)
        {
            var filename = string.Format("{0}/{1}.settings",_settingFoldername, settingsName);
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