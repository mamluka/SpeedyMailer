using SpeedyMailer.Core.Utilities.General;

namespace SpeedyMailer.Core.Utilities.Domain.Contacts
{
    public interface IContactsCSVParser : IReportResults<ContactCSVParserResults>
    {
        void ParseAndStore();
        void AddInitialContactBatchOptions(InitialContactsBatchOptions model);
    }
}