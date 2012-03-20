using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Document;
using SpeedyMailer.Core.Emails;

namespace SpeedyMailer.DbHandler
{
    class Program
    {
        static void Main(string[] args)
        {
            var store = new DocumentStore
                            {
                                Url = "http://localhost:8080"
                            };

            store.Initialize();

            using (var session = store.OpenSession())
            {
                var fragments = session.Query<EmailFragment>();
                fragments.ToList().ForEach(session.Delete);
                session.SaveChanges();
            }
            

        }
    }
}
