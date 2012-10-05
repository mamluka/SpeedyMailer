using System;
using System.Diagnostics;
using System.IO;
using Castle.DynamicProxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpeedyMailer.Core.Container
{
	public class JsonFileSettingsBinder : SettingsBinderBase
	{
		private readonly string _settingFoldername;

		public JsonFileSettingsBinder(string settingFoldername = "settings")
		{
			_settingFoldername = settingFoldername;
		}

		protected override object ReadPresistantSettings(string settingsName)
		{
			var filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _settingFoldername, string.Format("{0}.settings", settingsName));

			Trace.WriteLine("Settings were searched in:" + filename);

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
}