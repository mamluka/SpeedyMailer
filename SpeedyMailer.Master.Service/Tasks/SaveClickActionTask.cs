using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.Responses;
using Raven.Client;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Master.Service.Tasks
{
	public class SaveClickActionTask : PersistentTask
	{
		public DealUrlData Data { get; set; }
	}

	public class SaveClickActionTaskExecutor : PersistentTaskExecutor<SaveClickActionTask>
	{
		private readonly IDocumentStore _documentStore;

		public SaveClickActionTaskExecutor(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public override void Execute(SaveClickActionTask task)
		{
			using (var session = _documentStore.OpenSession())
			{
				var dealUrlData = task.Data;

				var creative = session.Load<Creative>(dealUrlData.CreativeId);

				var contactActions = session
					.Query<ContactActions>()
					.Customize(x => x.WaitForNonStaleResults(TimeSpan.FromMinutes(5)))
					.FirstOrDefault(q => q.ContactId == dealUrlData.ContactId);

				if (contactActions == null)
				{
					contactActions = new ContactActions
					{
						Clicks = new List<string> { dealUrlData.CreativeId },
						ContactId = dealUrlData.ContactId,
						Date = DateTime.UtcNow,
					};
				}
				else
				{
					contactActions.Clicks.Add(creative.Id);
				}

				session.Store(contactActions);
				session.SaveChanges();
			}
		}
	}
}
