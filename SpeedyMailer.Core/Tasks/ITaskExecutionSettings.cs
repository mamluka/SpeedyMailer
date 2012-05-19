using SpeedyMailer.Core.Settings;

namespace SpeedyMailer.Core.Tasks
{
	public interface ITaskExecutionSettings
	{
		[Default(3)]
		int NumberOfRetries { get; set; }
	}
}