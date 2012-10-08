using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
	}
}
