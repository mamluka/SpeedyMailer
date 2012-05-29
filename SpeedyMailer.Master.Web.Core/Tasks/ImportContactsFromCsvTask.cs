using System.IO;
using System.Linq;
using CsvHelper;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Web.Core.Commands;

namespace SpeedyMailer.Master.Web.Core.Tasks
{
	public class ImportContactsFromCsvTask : PersistentTask
	{
		public string ListId { get; set; }
		public string File { get; set; }
		public Results TaskResults { get; set; }

		public class Results
		{
			public string Filename { get; set; }
			public long NumberOfContacts { get; set; }
		}
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
			var rows = csvReader.GetRecords<ContactFromCSVRow>().ToList();

			var contacts = rows.Select(x => new Contact
												{
													Country = x.Country,
													Email = x.Email,
													Name = x.Name,
												});

			_addContactsCommand.Contacts = contacts;
			_addContactsCommand.ListId = task.ListId;

			var counter = _framework.ExecuteCommand(_addContactsCommand);

			var result = new ImportContactsFromCsvTask.Results
					   {
						   NumberOfContacts = counter,
						   Filename = Path.GetFileName(task.File)
					   };
			task.TaskResults = result;
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
