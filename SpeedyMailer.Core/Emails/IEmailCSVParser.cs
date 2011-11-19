using SpeedyMailer.Core.Core;

namespace SpeedyMailer.Core.Emails
{
    public interface IEmailCSVParser:IReportResults<MailCSVParserResults>
    {
        void ParseAndStore();
    }
}