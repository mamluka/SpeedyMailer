using NLog;
using Ninject.Modules;

namespace SpeedyMailer.Core.Logging
{
	public class LoggingModule : NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<Logger>().ToMethod(x =>
				{
					var className = x.Request.Target != null ? x.Request.Target.Member.DeclaringType.FullName : "SpeedyMailer.Unknown";
					return LogManager.GetLogger(className);
				});
		}
	}
}
