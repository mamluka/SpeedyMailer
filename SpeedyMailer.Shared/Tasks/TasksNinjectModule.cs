using Ninject.Modules;
using Quartz;

namespace SpeedyMailer.Shared.Tasks
{
	public class TasksNinjectModule:NinjectModule
	{
		public override void Load()
		{
			Kernel.Bind<IScheduler>().ToProvider<QuartzSchedulerProvider>();
		}
	}
}
