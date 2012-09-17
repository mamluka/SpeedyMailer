using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Nancy;
using Nancy.ModelBinding;
using Raven.Database.Exceptions;
using SpeedyMailer.Core.Apis;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Master.Service.Tasks;

namespace SpeedyMailer.Master.Service.Modules
{
    public class ContactsModule : NancyModule
    {
        private readonly Framework _framework;

        public ContactsModule(Framework framework):base("/contacts")
        {
            _framework = framework;
            Put["/upload-list"] = x =>
                                      {
                                          var model = this.Bind<ServiceEndpoints.UploadContacts>();
                                          var file = Request.Files.FirstOrDefault();
                                          if (file == null)
                                              throw new BadRequestException("no files was provided");

                                          string path = Guid.NewGuid().ToString();
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
        }
    }
}
