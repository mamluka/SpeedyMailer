using Quartz;

namespace SpeedyMailer.Core.Tasks
{
	public interface ITaskData<out T>
	{
		T GetData(IJobExecutionContext context);
	}
}