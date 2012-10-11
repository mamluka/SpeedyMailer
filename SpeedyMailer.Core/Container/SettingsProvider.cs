using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ninject;

namespace SpeedyMailer.Core.Container
{
	public class SettingsProvider
	{
		private readonly IKernel _kernel;

		public SettingsProvider(IKernel kernel)
		{
			_kernel = kernel;
		}

		public IList<Dictionary<string, object>> Settings()
		{
			return typeof(CoreAssemblyMarker).Assembly
				.GetExportedTypes()
				.Where(x => x.Name.EndsWith("Settings"))
				.Select(x => _kernel.Get(x))
				.Select(x =>
							 {
								 var type = x.GetType();
								 var prop = type.GetProperties();
								 return prop.Select(info => new
								 {
									 Value = info.GetValue(x,null),
									 info.Name
								 }).ToDictionary(key => key.Name, value => value.Value);
							 }).ToList();
		}
	}
}