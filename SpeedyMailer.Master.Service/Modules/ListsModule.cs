using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Raven.Client;
using Raven.Database.Exceptions;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Lists;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Commands;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Modules
{
	public class ListsModule : NancyModule
	{
		private readonly Framework _framework;
		private readonly IDocumentStore _documentStore;
		private readonly CreateListCommand _createListCommand;

		public ListsModule(Framework framework, IDocumentStore documentStore, CreateListCommand createListCommand)
			: base("/lists")
        {
		    _createListCommand = createListCommand;
		    _documentStore = documentStore;
		    _framework = framework;

            Put["/upload-list"] = api =>
                                      {
                                          var model = this.Bind<ServiceEndpoints.UploadContacts>();
                                          var file = Request.Files.FirstOrDefault();
                                          if (file == null)
                                              throw new BadRequestException("no files was provided");

                                          var path = Guid.NewGuid().ToString();
                                          using (var diskFile = File.OpenWrite(path))
                                          {
                                              file.Value.CopyTo(diskFile);
                                          }

                                          var importContactsFromCsvTask = new ImportContactsFromCsvTask
                                                                              {
                                                                                  File = path, ListId = model.ListName
                                                                              };
                                          _framework.ExecuteTask(importContactsFromCsvTask);

                                          return importContactsFromCsvTask.Id;
                                      };

		    Get["/"] = api =>
			                  {
				                  using (var session = _documentStore.OpenSession())
				                  {
					                  return Response.AsJson(session.Query<ListDescriptor>().ToList());
				                  }
			                  };

			Post["/"] = api =>
				                  {
					                  var model = this.Bind<ServiceEndpoints.CreateList>();
					                  _createListCommand.Name = model.Name;
					                  var listId = _framework.ExecuteCommand(_createListCommand);

					                  return Response.AsJson(new ApiStringResult {Result = listId});
				                  };
        }
	}
}
