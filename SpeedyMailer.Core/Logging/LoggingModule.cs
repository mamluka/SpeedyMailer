using NLog;
using Ninject.Modules;

namespace SpeedyMailer.Core.Logging
{
	public class LoggingModule : NinjectModule
	{
		public override void Load()
		{
			
			Kernel.Bind<Logger>().ToMethod(x => LogManager.GetLogger(x.Request.Target.Member.DeclaringType.FullName));
//			Kernel.Bind<Logger>().ToConstant(LogManager.GetCurrentClassLogger()).InSingletonScope();
		}
	}
}
