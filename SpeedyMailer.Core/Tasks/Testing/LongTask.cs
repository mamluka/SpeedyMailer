using System.Threading;

namespace SpeedyMailer.Core.Tasks.Testing
{
	public class LongTask:PersistentTask
	{
		public int Seconds { get; set; }
	}

	public class LongTaskExecutor:PersistentTaskExecutor<LongTask>
	{
		public override void Execute(LongTask task)
		{
			Thread.Sleep(task.Seconds*1000);
		}
	}
}