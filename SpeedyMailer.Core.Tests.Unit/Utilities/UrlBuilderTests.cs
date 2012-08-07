using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SpeedyMailer.Core.Utilities;
using SpeedyMailer.Tests.Core;
using Rhino.Mocks;
using FluentAssertions;
using SpeedyMailer.Tests.Core.Unit.Base;

namespace SpeedyMailer.Core.Tests.Unit.Utilities
{
	[TestFixture]
	public class UrlBuilderTests : AutoMapperAndFixtureBase
	{

		private UrlBuilder _target;

		[SetUp]
		public void Setup()
		{
			_target = new UrlBuilder();
		}

		[Test]
		public void AddObject_WhenGiveAnObject_ShouldAppendTheSerializedObjectToTheEndOfThePath()
		{
			var testObject = new ObjectWeWantToSerialize
			                 	{
			                 		Prop1 = "testing"
			                 	};
			var url = _target
				.Base("http://www.base.com")
				.AddObject(testObject)
				.AppendAsSlashes();

			url.Should().Be("http://www.base.com/" + Serialize(testObject));
		}

		private static string Serialize(ObjectWeWantToSerialize testObject)
		{
			return UrlBuilder.ToBase64(testObject);
		}
	}

	public class ObjectWeWantToSerialize
	{
		public string Prop1 { get; set; }
	}
}
