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
												   var fullName = x.Request.Target.Member.DeclaringType != null ? x.Request.Target.Member.DeclaringType.FullName : "RootType";
												   return LogManager.GetLogger(fullName);
											   });
			//			Kernel.Bind<Logger>().ToConstant(LogManager.GetCurrentClassLogger()).InSingletonScope();
		}
	}
}
