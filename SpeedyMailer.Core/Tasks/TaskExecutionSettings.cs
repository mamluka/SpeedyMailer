using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Core.Tasks
{
	public class TaskExecutionSettings
	{
		[Default(3)]
		public virtual int NumberOfRetries { get; set; }
	}
}