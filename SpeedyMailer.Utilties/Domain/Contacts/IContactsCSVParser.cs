using SpeedyMailer.Utilties.General;

namespace SpeedyMailer.Utilties.Domain.Contacts
{
    public interface IContactsCSVParser : IReportResults<ContactCSVParserResults>
    {
        void ParseAndStore();
        void AddInitialContactBatchOptions(InitialContactsBatchOptions model);
    }
}