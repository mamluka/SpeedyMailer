using AutoMapper;
using SpeedyMailer.Core.Contacts;
using SpeedyMailer.Master.Web.Core.ViewModels;

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