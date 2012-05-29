using Raven.Client;

namespace SpeedyMailer.Core.Tasks
{
	public abstract class PersistentTaskExecutor<T>
	{
		public abstract void Execute(T task);
	}

	public abstract class PersistentTaskExecutorWithResult<T>:PersistentTaskExecutor<T>
	{
		private readonly IDocumentStore _documentStore;

		protected PersistentTaskExecutorWithResult(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public void UpdateTask(PersistentTask task)
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(task);
				session.SaveChanges();
			}
		}
	}
}