using System;

namespace SpeedyMailer.Core.Tasks
{
	public abstract class PersistentTask
	{
		public string Id { get; set; }
		public DateTime CreateDate { get; set; }
		public PersistentTaskStatus Status { get; set; }
		public int RetryCount { get; set; }
	}

	public enum PersistentTaskStatus
	{
		Pending=0,
		Executing=1,
		Executed=2,
		Failed=3
	}
}