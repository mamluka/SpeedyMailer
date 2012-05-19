namespace SpeedyMailer.Core.Tasks
{
	public abstract class PersistentTaskExecutor<T>
	{
		public abstract void Execute(T task);
	}
}