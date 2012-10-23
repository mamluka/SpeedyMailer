using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Nancy.Bootstrapper;
using Nancy.Bootstrappers.Ninject;
using Ninject;
using Ploeh.AutoFixture;
using Raven.Client;
using Raven.Client.Document;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Settings;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Master.Service;
using SpeedyMailer.Master.Service.Commands;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class UiActions : MasterActionsBase
	{
		public UiActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor, IScheduledTaskManager scheduledTaskManager)
			: base(kernel, taskManager, taskExecutor, scheduledTaskManager)
		{ }

		public string CreateListWithRandomContacts(string listName, int contactsCount)
		{
			var listId = ExecuteCommand<CreateListCommand, string>(x => x.Name = listName);
			ExecuteCommand<AddContactsCommand, long>(command =>
			{
				command.Contacts = Fixture
					.Build<Contact>()
					.Without(x => x.Id)
					.CreateMany(contactsCount);

				command.ListId = listId;

			});
			return listId;
		}

		public string CreateListWithContacts(string listName, IEnumerable<Contact> contacts)
		{
			var listId = ExecuteCommand<CreateListCommand, string>(x => x.Name = listName);
			ExecuteCommand<AddContactsCommand, long>(command =>
														 {
															 command.Contacts = contacts;
															 command.ListId = listId;
														 });
			return listId;
		}

		public string CreateSimpleCreative(IEnumerable<string> lists, string unsubscribeTemplateId)
		{
			return ExecuteCommand<AddCreativeCommand, string>(x =>
			{
				x.Body = "body";
				x.Lists = lists.ToList();
				x.Subject = "Subject";
				x.UnsubscribeTemplateId = unsubscribeTemplateId;
				x.DealUrl = "dealUrl";
				x.FromName = "david";
				x.FromAddressDomainPrefix = "sales";
			});
		}
	}

	public class ServiceActions : MasterActionsBase
	{
		private TopService _topService;

		public ServiceActions(IKernel kernel, ITaskManager taskManager, ITaskExecutor taskExecutor, IScheduledTaskManager scheduledTaskManager)
			: base(kernel, taskManager, taskExecutor, scheduledTaskManager)
		{ }

		public void Initialize()
		{
			var documentStore = Kernel.Get<IDocumentStore>();

			Kernel.Rebind<INancyBootstrapper>().ToConstant(new ServiceNancyNinjectBootstrapperForTesting(documentStore) as INancyBootstrapper);
			_topService = Kernel.Get<TopService>();
		}
		public void Start()
		{
			_topService.Start();
		}

		public void Stop()
		{
			if (_topService != null)
				_topService.Stop();
		}
	}
}
