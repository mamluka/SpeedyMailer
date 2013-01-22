using CsvHelper.Configuration;

namespace SpeedyMailer.Core.Domain.Contacts
{
    public class ContactsListCsvRow
    {
        [CsvField(Index = 0)]
        public string Email { get; set; }
		
		[CsvField(Index = 1)]
        public string Firstname { get; set; }

		[CsvField(Index = 2)]
		public string Lastname { get; set; }

        [CsvField(Index = 3)]
        public string City { get; set; }

		[CsvField(Index = 4)]
		public string State { get; set; }

	    [CsvField(Index = 5)]
	    public string Country { get; set; }

	    [CsvField(Index = 6)]
		public string Zip { get; set; }

	    [CsvField(Index = 7)]
		public string Phone { get; set; }

	    [CsvField(Index = 8)]
		public string Ip { get; set; }
    }
	
	public class OneRawContactsListCsvRow
    {
		protected bool Equals(OneRawContactsListCsvRow other)
		{
			return string.Equals(Email, other.Email);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((OneRawContactsListCsvRow) obj);
		}

		public override int GetHashCode()
		{
			return (Email != null ? Email.GetHashCode() : 0);
		}

		[CsvField(Index = 0)]
        public string Email { get; set; }

		[CsvField(Ignore = true)]
		public bool Mark { get; set; }
    }
}