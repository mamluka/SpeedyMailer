using Raven.Client;

namespace SpeedyMailer.Core.Tasks.Testing
{
	public class RaceConditionTaskExecutor : PersistentTaskExecutor<RaceConditionTask>
	{
		private readonly IDocumentStore _documentStore;

		public RaceConditionTaskExecutor(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute(RaceConditionTask task)
		{
			using (var session = _documentStore.OpenSession())
			{
				var result = session.Load<ComputationResult<int>>(task.ResultId) ?? NewResult(task.ResultId);
				result.Result++;
				session.Store(result);
				session.SaveChanges();
			}
		}

		private static ComputationResult<int> NewResult(string resultId)
		{
			return new ComputationResult<int>
			       	{
						Id = resultId,
			       		Result = 0
			       	};
		}
	}
}