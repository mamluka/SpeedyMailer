using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Web;
using System.Linq;
using AutoMapper;
using CsvHelper;

namespace SpeedyMailer.Core.Emails
{
    public class EmailCSVParser : IEmailCSVParser
    {
        private readonly HttpContextBase httpContextBase;
        private readonly IEmailsRepository emailsRepository;
        private readonly IMappingEngine mapper;
        private MailCSVParserResults results;

        public EmailCSVParser(HttpContextBase httpContextBase, IEmailsRepository emailsRepository, IMappingEngine mapper)
        {
            this.httpContextBase = httpContextBase;
            this.emailsRepository = emailsRepository;
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

            var emails = new List<EmailFromCSVRow>();

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
                
                var csvHelper = new CsvHelper.CsvHelper(mStream);
                
                 emails.AddRange(csvHelper.Reader.GetRecords<EmailFromCSVRow>());
            }

            var emailsDTO = mapper.Map<List<EmailFromCSVRow>, List<Email>>(emails);

            emailsRepository.Store(emailsDTO);

            results = new MailCSVParserResults()
                          {
                              NumberOfFilesProcessed = files.Count,
                              NumberOfEmailProcessed = emails.Count,
                              Filenames = files.Select(x=> x.FileName).ToList()
                              

                          };

        }

        public MailCSVParserResults Results
        {
            get { return results; }
        }
    }
}