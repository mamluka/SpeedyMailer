using System;
using Raven.Client;

namespace SpeedyMailer.Core.Tasks
{
	public interface ITaskManager
	{
		string Save(PersistentTask persistentTask);
	}

	public class TaskManager : ITaskManager
	{
		private readonly IDocumentStore _documentStore;

		public TaskManager(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public string Save(PersistentTask persistentTask)
		{
			using (var session = _documentStore.OpenSession())
			{
				persistentTask.CreateDate = DateTime.UtcNow;

				session.Store(persistentTask);
				session.SaveChanges();

				return persistentTask.Id;
			}
		}
	}
}