using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Linq;
using AutoMapper;
using CsvHelper;
using SpeedyMailer.Core.DataAccess.Contacts;
using SpeedyMailer.Domain.Model.Contacts;

namespace SpeedyMailer.Core.Contacts
{
    public class ContactsCSVParser : IContactsCSVParser
    {
        private InitialContactsBatchOptions initialContactsBatchOptions;
        private readonly HttpContextBase httpContextBase;
        private readonly IContactsRepository contactsRepository;
        private readonly IMappingEngine mapper;
        private ContactCSVParserResults results;
        private bool hasInitialEmailBatchOptions;

        public ContactsCSVParser(HttpContextBase httpContextBase, IContactsRepository contactsRepository, IMappingEngine mapper)
        {
            this.httpContextBase = httpContextBase;
            this.contactsRepository = contactsRepository;
            this.mapper = mapper;
        }

        public void ParseAndStore()
        {
            if (httpContextBase.Request.Files.Count == 0)
            {
                throw new Exception("when asked to Parse And Store the Uploaded CSV there were no files in the request");
            }
            var files = new List<HttpPostedFileBase>();

          
            

            for (int i = 0; i < httpContextBase.Request.Files.Count; i++)
            {
                files.Add(httpContextBase.Request.Files[i]);
            }

            var emails = new List<ContactFromCSVRow>();

            foreach (var file in files)
            {
                if (file.InputStream.Length == 0)
                {
                    throw new Exception("when asked to Parse the Uploaded CSV there was no data in filename:" + file.FileName);
                }
                var mStream = new MemoryStream();
                file.InputStream.CopyTo(mStream);
                mStream.Flush();
                mStream.Position = 0;


                var csvParser = new CsvParser(new StreamReader(mStream));
                var csvReader = new CsvReader(csvParser);
                
                 emails.AddRange(csvReader.GetRecords<ContactFromCSVRow>());
            }

            var emailsDTO = mapper.Map<List<ContactFromCSVRow>, List<Contact>>(emails);

            if (hasInitialEmailBatchOptions)
            {
                 emailsDTO.ForEach(x => x.MemberOf.Add(initialContactsBatchOptions.ContainingListId));
            }

           

            contactsRepository.Store(emailsDTO);

            results = new ContactCSVParserResults()
                          {
                              NumberOfFilesProcessed = files.Count,
                              NumberOfContactsProcessed = emails.Count,
                              Filenames = files.Select(x=> x.FileName).ToList()
                              

                          };

        }

        public ContactCSVParserResults Results
        {
            get { return results; }
        }

        public void AddInitialContactBatchOptions(InitialContactsBatchOptions model)
        {
            initialContactsBatchOptions = model;
            hasInitialEmailBatchOptions = true;
        }
    }
}