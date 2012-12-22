using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using SpeedyMailer.Core;
using SpeedyMailer.Core.Utilities;

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

		public static string DefaultStoreUri(int port = 27027)
		{
			return "mongodb://localhost:" + port + "/drone?safe=true";
		}

		public static string RandomDefaultStoreUri(int port = 27027)
		{
			return DefaultStoreUri(RandomNumber());
		}

		public static string GenerateRandomLocalhostAddress()
		{
			return "http://localhost:" + RandomNumber();
		}

		public static void ValidateSettingClasses()
		{
			var settings = typeof(CoreAssemblyMarker)
				.Assembly.GetExportedTypes()
				.Where(x => x.Name.EndsWith("Settings"))
				.SelectMany(x => x.GetProperties())
				.Where(x => !x.GetGetMethod().IsVirtual)
				.Select(x => new { x.DeclaringType, x.Name })
				.ToList();

			if (settings.Any())
			{
				var setting = settings.First();
				Assert.Fail("You have a settings class {0} with method {1} that is not virtual", setting.DeclaringType, setting.Name);
			}
		}

		public static string Encode(object obj)
		{
			return UrlBuilder.ToBase64(obj);
		}
		
		public static string Encode(string obj)
		{	
			return UrlBuilder.ToBase64(obj);
		}

		private static int RandomNumber()
		{
			return new Random().Next(2000, 10000);
		}

		public static int RandomPort()
		{
			return RandomNumber();
		}
	}
}
