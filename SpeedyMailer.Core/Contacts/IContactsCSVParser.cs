using SpeedyMailer.Core.Core;

namespace SpeedyMailer.Core.Contacts
{
    public interface IContactsCSVParser:IReportResults<ContactCSVParserResults>
    {
        void ParseAndStore();
        void AddInitialContactBatchOptions(InitialContactsBatchOptions model);
    }
}