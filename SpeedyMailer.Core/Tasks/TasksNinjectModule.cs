using Ninject.Modules;
using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public class TasksNinjectModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IScheduler>().ToProvider<QuartzSchedulerProvider>();
		}
	}
}
