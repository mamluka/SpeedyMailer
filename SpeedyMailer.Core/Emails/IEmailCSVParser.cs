using SpeedyMailer.Core.Core;

namespace SpeedyMailer.Core.Emails
{
    public interface IEmailCSVParser:IReportResults<EmailCSVParserResults>
    {
        void ParseAndStore();
        void AddInitialEmailBatchOptions(InitialEmailBatchOptions model);
    }
}