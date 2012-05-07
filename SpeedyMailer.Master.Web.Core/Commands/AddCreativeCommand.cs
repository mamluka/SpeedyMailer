using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using SpeedyMailer.Core.Commands;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Web.Core.Commands
{
    public class AddCreativeCommand:Command<string>
    {
        private IDocumentStore _store;

        public AddCreativeCommand(IDocumentStore store)
        {
            _store = store;
        }

        public override string Execute()
        {
            using (var session = _store.OpenSession())
            {
                var creative = new Creative
                                   {
                                       Body = Body,
                                       Lists = Lists,
                                       Subject = Subject,
                                       UnsubscribeTemplateId = UnsubscribeTemplateId
                                   };
                session.Store(creative);
                session.SaveChanges();

                return creative.Id;
            }
        }

        public string Body { get; set; }
        public string Subject { get; set; }
        public List<string> Lists { get; set; }
        public string UnsubscribeTemplateId { get; set; }
    }
}
