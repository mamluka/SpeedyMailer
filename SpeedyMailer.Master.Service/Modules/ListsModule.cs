using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.ModelBinding;
using Raven.Client;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;
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
                                          var model = this.Bind<ServiceEndpoints.Lists.UploadContacts>();
                                          var file = Request.Files.FirstOrDefault();
                                          if (file == null)
                                              throw new ArgumentNullException("no files was provided");

                                          var path = Guid.NewGuid().ToString();
                                          using (var diskFile = File.OpenWrite(path))
                                          {
                                              file.Value.CopyTo(diskFile);
                                          }

                                          var importContactsFromCsvTask = new ImportContactsFromCsvTask
                                                                              {
                                                                                  File = path, ListId = model.ListId
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
					                  var model = this.Bind<ServiceEndpoints.Lists.CreateList>();
					                  _createListCommand.Name = model.Name;
					                  var listId = _framework.ExecuteCommand(_createListCommand);

					                  return Response.AsJson(new ApiStringResult {Result = listId});
				                  };

			Get["/unsubscribe/{data}"] = call =>
				                             {
					                             var dataString = call.data;
					                             var data = UrlBuilder.DecodeBase64<DealUrlData>(dataString);

												 using (var session = documentStore.OpenSession())
					                             {
						                             session.Store(new UnsubscribeRequest
							                                           {
								                                           ContactId = data.ContactId,
																		   CreativeId = data.CreativeId
							                                           });

													 session.SaveChanges();
					                             }

					                             return "You were successfully unsubscribed from the list, thank you";
				                             };
        }
	}
}
