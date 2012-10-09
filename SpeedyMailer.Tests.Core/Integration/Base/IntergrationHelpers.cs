using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SpeedyMailer.Core;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public static class IntergrationHelpers
	{
		public static string AssemblyDirectory
		{
			get
			{
				var codeBase = Assembly.GetExecutingAssembly().CodeBase;
				var uri = new UriBuilder(codeBase);
				var path = Uri.UnescapeDataString(uri.Path);
				return Path.GetDirectoryName(path);
			}
		}

		public static string GenerateRandomLocalhostAddress()
		{
			return "http://localhost:" + DateTime.Now.Second + DateTime.Now.Millisecond;
		}

		public static void ValidateSettingClasses()
		{
			var settings = typeof (CoreAssemblyMarker)
				.Assembly.GetExportedTypes()
				.Where(x => x.Name.EndsWith("Settings"))
				.SelectMany(x => x.GetProperties())
				.Where(x => !x.GetGetMethod().IsVirtual)
				.Select(x => new { x.DeclaringType, x.Name })
				.ToList();

			if (settings.Any())
			{
				var setting = settings.First();
				NUnit.Framework.Assert.Fail("You have a settings class {0} with method {1} that is not virtual", setting.DeclaringType, setting.Name);
			}
		}
	}
}
