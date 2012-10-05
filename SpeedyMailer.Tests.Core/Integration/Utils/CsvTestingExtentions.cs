using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Tests.Core.Integration.Utils
{
	public static class CsvTestingExtentions
	{
		public static void ToCsvFile<T>(this IEnumerable<T> target, string filename) where T:class 
		{
			using (var textWriter = new StreamWriter(filename))
			{
				var csvWriter = new CsvWriter(textWriter);
				csvWriter.WriteRecords(target.ToList());
			}
		}

		public static string GenerateFileName(string seed)
		{
			return string.Format("{0}-{1}.{2}", seed, Guid.NewGuid(), "csv");
		}

	}
}
