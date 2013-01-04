using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;

namespace SpeedyMailer.Tests.Performance.Raven
{
	public class RavenSizesTest
	{
		[Test]
		public void Given_WhenWhen_ShouldExpectedResult()
		{
			var documentStore = new DocumentStore { Url = "http://mamluka-pc:4253" };
			documentStore.Initialize();


			var fixture = new Fixture();

			using (var session = documentStore.OpenSession())
			{
				var theList = fixture.CreateMany<Contact>(1000).ToList();

				theList.Take(500).ToList().ForEach(x => x.DomainGroup = "a");
				theList.Skip(500).Take(500).ToList().ForEach(x => x.DomainGroup = "b");

				var bigDoc = new Holder
								 {
									 TheList = theList
								 };

				session.Store(bigDoc);
				session.SaveChanges();
			}

			documentStore.DatabaseCommands.PutIndex("Count_Contacts", new IndexDefinitionBuilder<Holder, SummeryOfDomains>
			{
				Map = contacts => contacts.SelectMany(x => x.TheList).Select(x => new { x.DomainGroup, Count = 1 }),
				Reduce = results => results.GroupBy(x => x.DomainGroup).Select(x => new { DomainGroup = x.Key, Count = x.Sum(m => m.Count) })

			}, true);

			using (var session = documentStore.OpenSession())
			{
				Trace.WriteLine(session.Query<List<int>>("Count_Contacts").First());
			}
		}

		[Test]
		public void Given_WhenWhen_ShouldExpectedResultdfdd()
		{
			var documentStore = new DocumentStore { Url = "http://mamluka-pc:4253" };
			documentStore.Initialize();

			documentStore.DatabaseCommands.PutIndex("Count_Contacts", new IndexDefinitionBuilder<Contact>
			{
				Map = contacts => contacts.Select(x => new { x.MemberOf })

			}, true);

		}

		[Test]
		public void Given_WhenWhensdfdsdf_ShouldExpectedResultdfdd()
		{
			var documentStore = new DocumentStore { Url = "http://mamluka-pc:4253" };
			documentStore.Initialize();

			documentStore.DatabaseCommands.PutIndex("Count_Contacts", new IndexDefinitionBuilder<Contact>
			{
				Map = contacts => contacts.Select(x => new { x.MemberOf })

			}, true);

		}

		[Test]
		public void DNS_WhenWhen_ShouldExpectedResult()
		{
			Trace.WriteLine(Dns.GetHostEntry("88.213.179.79.zen.spamhaus.org").AddressList.First());
			Trace.WriteLine(Dns.GetHostEntry("66.124.7.195.bl.spamcop.net").AddressList.First());
		}
	}

	public class SummeryOfDomains
	{
		public string DomainGroup { get; set; }
		public int Count { get; set; }
	}

	public class Holder
	{
		public IEnumerable<Contact> TheList { get; set; }
	}
}
