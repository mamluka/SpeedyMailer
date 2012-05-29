using Newtonsoft.Json;
using Quartz;

namespace SpeedyMailer.Drone.Tasks
{
	public abstract class JobBase<T> : ITaskData<T>
	{
		public T GetData(IJobExecutionContext context)
		{
			var data =  context.JobDetail.JobDataMap["data"] as string;
			return JsonConvert.DeserializeObject<T>(data);
		}
	}
}