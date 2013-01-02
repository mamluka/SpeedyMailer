using System;
using System.Collections.Specialized;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Changes;
using Raven.Client.Connection;
using Raven.Client.Connection.Async;
using Raven.Client.Document;
using Raven.Client.Indexes;

namespace SpeedyMailer.Core.Container
{
	public class NoRavenSupport:IDocumentStore
	{
		public void Dispose()
		{
			throw new NotImplementedException();
		}

		public bool WasDisposed { get; private set; }
		public event EventHandler AfterDispose;
		public IDatabaseChanges Changes(string database = null)
		{
			throw new NotImplementedException();
		}

		public IDisposable AggressivelyCacheFor(TimeSpan cahceDuration)
		{
			throw new NotImplementedException();
		}

		public IDisposable DisableAggressiveCaching()
		{
			throw new NotImplementedException();
		}

		public IDocumentStore Initialize()
		{
			throw new NotImplementedException();
		}

		public IAsyncDocumentSession OpenAsyncSession()
		{
			throw new NotImplementedException();
		}

		public IAsyncDocumentSession OpenAsyncSession(string database)
		{
			throw new NotImplementedException();
		}

		public IDocumentSession OpenSession()
		{
			throw new NotImplementedException();
		}

		public IDocumentSession OpenSession(string database)
		{
			throw new NotImplementedException();
		}

		public IDocumentSession OpenSession(OpenSessionOptions sessionOptions)
		{
			throw new NotImplementedException();
		}

		public void ExecuteIndex(AbstractIndexCreationTask indexCreationTask)
		{
			throw new NotImplementedException();
		}

		public Guid? GetLastWrittenEtag()
		{
			throw new NotImplementedException();
		}

		public BulkInsertOperation BulkInsert(string database = null, BulkInsertOptions options = null)
		{
			throw new NotImplementedException();
		}

		public NameValueCollection SharedOperationsHeaders { get; private set; }
		public HttpJsonRequestFactory JsonRequestFactory { get; private set; }
		public string Identifier { get; set; }
		public IAsyncDatabaseCommands AsyncDatabaseCommands { get; private set; }
		public IDatabaseCommands DatabaseCommands { get; private set; }
		public DocumentConvention Conventions { get; private set; }
		public string Url { get; private set; }
	}
}