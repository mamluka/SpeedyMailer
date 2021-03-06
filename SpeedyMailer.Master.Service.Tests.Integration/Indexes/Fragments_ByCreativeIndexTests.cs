﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Master.Service.Storage.Indexes;
using SpeedyMailer.Tests.Core.Integration.Base;

namespace SpeedyMailer.Master.Service.Tests.Integration.Indexes
{
	public class Fragments_ByCreativeIndexTests : IntegrationTestBase
	{
		[Test]
		public void Index_WhenCalled_ShouldMapReduceTheFragments()
		{
			var fragments = new[]
				{
					new CreativeFragment {Id = "CreativeFragments/1", CreativeId = "creatives/1", Status = FragmentStatus.Pending, FetchedAt = new DateTime(2000,1,1,0,0,0), FetchedBy = "drone1"},
					new CreativeFragment {Id = "CreativeFragments/2", CreativeId = "creatives/1", Status = FragmentStatus.Sending, FetchedAt = new DateTime(2001,1,1,0,0,0), FetchedBy = "drone2"},
					new CreativeFragment {Id = "CreativeFragments/3", CreativeId = "creatives/2", Status = FragmentStatus.Pending, FetchedAt = new DateTime(2002,1,1,0,0,0), FetchedBy = "drone2"},
					new CreativeFragment {Id = "CreativeFragments/4", CreativeId = "creatives/2", Status = FragmentStatus.Sending, FetchedAt = new DateTime(2003,1,1,0,0,0), FetchedBy = "drone2"},
				}.ToList();

			fragments.ForEach(Store.Store);

			Store.WaitForIndexNotToBeStale<Fragments_ByCreative.ReduceResult, Fragments_ByCreative>();

			var result = Store.Query<Fragments_ByCreative.ReduceResult, Fragments_ByCreative>(x => x.CreativeId == "creatives/1");

			result[0].FragmentStatus.Should().Contain(x => x == "CreativeFragments/1: Pending at: 01/01/2000 00:00:00 by: drone1");
			result[0].FragmentStatus.Should().Contain(x => x == "CreativeFragments/2: Sending at: 01/01/2001 00:00:00 by: drone2");
		}
	}
}
