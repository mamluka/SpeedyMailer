using System;

namespace SpeedyMailer.Core.Tasks
{
	public abstract class PersistentTask
	{
		public string Id { get; set; }
		public DateTime CreateDate { get; set; }
		public bool Executed { get; set; }
	}
}