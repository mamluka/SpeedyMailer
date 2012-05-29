using Quartz;

namespace SpeedyMailer.Drone.Tasks
{
	public interface ITaskData<out T>
	{
		T GetData(IJobExecutionContext context);
	}
}