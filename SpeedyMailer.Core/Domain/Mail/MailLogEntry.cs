using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using Mongol;

namespace SpeedyMailer.Core.Domain.Mail
{
	[CollectionName("log")]
	public class MailLogEntry
	{
		[BsonId(IdGenerator = typeof(BsonObjectIdGenerator))]
		public virtual BsonObjectId Id { get; set; }

		public string msg { get; set; }
		public string level { get; set; }
		public DateTime time { get; set; }
		public string sys { get; set; }
		public DateTime time_rcvd { get; set; }
		public int syslog_fac { get; set; }
		public int syslog_sever { get; set; }
		public string syslog_tag { get; set; }
		public string procid { get; set; }
		public string pid { get; set; }
	}

	public class NewBsonObjectIdGenerator : IIdGenerator
	{
		// private static fields
		private static NewBsonObjectIdGenerator __instance = new NewBsonObjectIdGenerator();

		// constructors
		/// <summary>
		/// Initializes a new instance of the BsonObjectIdGenerator class.
		/// </summary>
		public NewBsonObjectIdGenerator()
		{
		}

		// public static properties
		/// <summary>
		/// Gets an instance of ObjectIdGenerator.
		/// </summary>
		public static NewBsonObjectIdGenerator Instance
		{
			get { return __instance; }
		}

		// public methods
		/// <summary>
		/// Generates an Id for a document.
		/// </summary>
		/// <param name="container">The container of the document (will be a MongoCollection when called from the C# driver). </param>
		/// <param name="document">The document.</param>
		/// <returns>An Id.</returns>
		public object GenerateId(object container, object document)
		{
			return BsonObjectId.GenerateNewId();
		}

		/// <summary>
		/// Tests whether an Id is empty.
		/// </summary>
		/// <param name="id">The Id.</param>
		/// <returns>True if the Id is empty.</returns>
		public bool IsEmpty(object id)
		{
			return id == null || ((BsonValue)id).IsBsonNull || ((BsonObjectId)id).Value == ObjectId.Empty;
		}
	}
}