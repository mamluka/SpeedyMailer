using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using AutoMapper;
using CsvHelper;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Core.Utilities.Domain.Contacts
{
    public class ContactsCSVParser : IContactsCSVParser
    {
        private readonly HttpContextBase httpContextBase;
        private readonly IMappingEngine mapper;
        private bool hasInitialEmailBatchOptions;
        private InitialContactsBatchOptions initialContactsBatchOptions;
        private ContactCSVParserResults results;

        public ContactsCSVParser(HttpContextBase httpContextBase,
                                 IMappingEngine mapper)
        {
            this.httpContextBase = httpContextBase;
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

            foreach (HttpPostedFileBase file in files)
            {
                if (file.InputStream.Length == 0)
                {
                    throw new Exception("when asked to Parse the Uploaded CSV there was no data in filename:" +
                                        file.FileName);
                }
                var mStream = new MemoryStream();
                file.InputStream.CopyTo(mStream);
                mStream.Flush();
                mStream.Position = 0;


                var csvParser = new CsvParser(new StreamReader(mStream));
                var csvReader = new CsvReader(csvParser);

                emails.AddRange(csvReader.GetRecords<ContactFromCSVRow>());
            }

//            List<Contact> emailsDTO = mapper.Map<List<ContactFromCSVRow>, List<Contact>>(emails);
//
//            if (hasInitialEmailBatchOptions)
//            {
//                emailsDTO.ForEach(x => x.MemberOf.Add(initialContactsBatchOptions.ContainingListId));
//            }
//
//
//            contactsRepository.Store(emailsDTO);

            results = new ContactCSVParserResults
                          {
                              NumberOfFilesProcessed = files.Count,
                              NumberOfContactsProcessed = emails.Count,
                              Filenames = files.Select(x => x.FileName).ToList<string>()
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