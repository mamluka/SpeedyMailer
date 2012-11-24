using System;
using System.IO;
using System.Linq;
using CsvHelper;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;

namespace SpeedyMailer.Master.Service.Tasks
{
	public class ImportContactsFromCsvTask : PersistentTask
	{
		public string ListId { get; set; }
		public string File { get; set; }
	}

	public class ImportContactsFromCsvTaskExecutor : PersistentTaskExecutor<ImportContactsFromCsvTask>
	{
		private readonly AddContactsCommand _addContactsCommand;
		private readonly Framework _framework;

		public ImportContactsFromCsvTaskExecutor(Framework framework, AddContactsCommand addContactsCommand)
		{
			_framework = framework;
			_addContactsCommand = addContactsCommand;
		}

		public override void Execute(ImportContactsFromCsvTask task)
		{
			var csvSource = File.OpenRead(task.File);
			var csvReader = new CsvReader(new StreamReader(csvSource));
			var rows = csvReader.GetRecords<ContactsListCsvRow>()
				.Distinct(new LambdaComparer<ContactsListCsvRow>((x, y) => x.Email == y.Email))
				.ToList();

			var contacts = rows.Select(x => new Contact
												{
													Country = x.Country,
													Email = x.Email.Trim(),
													Name = string.Format("{0} {1}", x.Firstname, x.Lastname),
													City = x.City,
													Ip = x.Ip,
													Phone = x.Phone,
													State = x.State,
													Zip = x.Zip,
												});

			_addContactsCommand.Contacts = contacts;
			_addContactsCommand.ListId = task.ListId;

			_framework.ExecuteCommand(_addContactsCommand);
		}
	}

	public class UniqueContactEnforcer : UniqueEnforcer
	{
		public UniqueContactEnforcer(string uniqueId, string enforcedId)
			: base(uniqueId, enforcedId)
		{ }
	}

	public class UniqueEnforcer
	{
		public string Id { get; set; }
		public string EnforcedId { get; set; }

		public UniqueEnforcer(string uniqueId, string enforcedId)
		{
			Id = uniqueId;
			EnforcedId = enforcedId;
		}
	}



}
