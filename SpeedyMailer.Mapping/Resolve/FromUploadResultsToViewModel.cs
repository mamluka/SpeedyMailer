using AutoMapper;
using SpeedyMailer.Master.Web.Core.ViewModels;
using SpeedyMailer.Utilties.Domain.Contacts;

namespace SpeedyMailer.Mapping.Resolve
{
    public class FromUploadResultsToViewModel :
        ValueResolver<ContactCSVParserResults, ContactsCSVParserResultsViewModel>
    {
        protected override ContactsCSVParserResultsViewModel ResolveCore(ContactCSVParserResults source)
        {
            return new ContactsCSVParserResultsViewModel
                       {
                           Filenames = source.Filenames,
                           NumberOfEmailProcessed = source.NumberOfContactsProcessed.ToString(),
                           NumberOfFilesProcessed = source.NumberOfFilesProcessed.ToString()
                       };
        }
    }
}