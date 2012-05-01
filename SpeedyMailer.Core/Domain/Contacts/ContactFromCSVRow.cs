using CsvHelper.Configuration;

namespace SpeedyMailer.Core.Domain.Contacts
{
    public class ContactFromCSVRow
    {
        [CsvField(Index = 0)]
        public string Email { get; set; }

        [CsvField(Index = 1)]
        public string Name { get; set; }

        [CsvField(Index = 2)]
        public string Country { get; set; }
    }
}