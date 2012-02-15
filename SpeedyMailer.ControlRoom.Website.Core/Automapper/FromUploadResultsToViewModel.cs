using AutoMapper;
using SpeedyMailer.ControlRoom.Website.Core.ViewModels;
using SpeedyMailer.Core.Contacts;

namespace SpeedyMailer.ControlRoom.Website.Core.Automapper
{
    public class FromUploadResultsToViewModel : ValueResolver<ContactCSVParserResults, ContactsCSVParserResultsViewModel>
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