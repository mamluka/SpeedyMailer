using SpeedyMailer.Core.Utilities.General;

namespace SpeedyMailer.Core.Utilities.Domain.Contacts
{
    public interface IContactsCsvParser : IReportResults<ContactCsvParserResults>
    {
        void ParseAndStore();
        void AddInitialContactBatchOptions(InitialContactsBatchOptions model);
    }
}