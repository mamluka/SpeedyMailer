using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Indexes;
using SpeedyMailer.Core.Domain.Contacts;
using SpeedyMailer.Core.Domain.Creative;

namespace SpeedyMailer.Master.Service.Storage.Indexes
{
	public class Fragments_ByCreative : AbstractIndexCreationTask<CreativeFragment, Fragments_ByCreative.ReduceResult>
	{
		public class ReduceResult
		{
			public string CreativeId { get; set; }
			public List<string> FragmentStatus { get; set; }
		}
		public Fragments_ByCreative()
		{
			Map = fragments => fragments.Select(x => new { x.CreativeId, FragmentStatus = new[] { x.Id + ": " + x.Status.ToString() } });

			Reduce = results => results
				                    .GroupBy(x => x.CreativeId)
				                    .Select(x => new {CreativeId = x.Key, FragmentStatus = x.SelectMany(result => result.FragmentStatus).ToList()});
		}
	}
}
