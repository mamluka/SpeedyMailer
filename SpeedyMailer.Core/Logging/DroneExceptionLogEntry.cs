using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace SpeedyMailer.Core.Logging
{
	public class DroneExceptionLogEntry
	{
		[BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
		public BsonObjectId id { get; set; }

		public string message { get; set; }
		public string exception { get; set; }
		public DateTime time { get; set; }
		public string component { get; set; }
	}
}
