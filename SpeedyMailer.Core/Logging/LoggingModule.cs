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
												   var className = x.Request != null ? x.Request.Target.Member.DeclaringType.FullName : "RootType";
												   return LogManager.GetLogger(className);
											   });
		}
	}
}
