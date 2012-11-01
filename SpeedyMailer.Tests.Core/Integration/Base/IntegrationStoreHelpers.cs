using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using EqualityComparer;
using Raven.Client;
using SpeedyMailer.Core.Tasks;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
	public class IntegrationStoreHelpers
	{
		private readonly IDocumentStore _documentStore;

		public IntegrationStoreHelpers(IDocumentStore documentStore)
		{
			_documentStore = documentStore;
		}

		public void Store(object item)
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(item);
				session.SaveChanges();
			}
		}

		public void Store(object item, string id)
		{
			using (var session = _documentStore.OpenSession())
			{
				session.Store(item, id);
				session.SaveChanges();
			}
		}

		public bool Compare<T>(T first, T second)
		{
			return MemberComparer.Equal(first, second);
		}

		public T Load<T>(string id)
		{
			using (var session = _documentStore.OpenSession())
			{
				return session.Load<T>(id);
			}
		}

		public IList<T> Query<T>(Expression<Func<T, bool>> expression)
		{
			using (var session = _documentStore.OpenSession())
			{
				return session.Query<T>().Where(expression).ToList();
			}
		}

		public IList<T> Query<T>()
		{
			using (var session = _documentStore.OpenSession())
			{
				return session.Query<T>().Take(1024).ToList();
			}
		}

		public void Delete<T>(string entityId)
		{
			using (var session = _documentStore.OpenSession())
			{
				var entity = session.Load<T>(entityId);
				session.Delete(entity);
				session.SaveChanges();
			}
		}


		public void WaitForEntityToExist(string entityId, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
					session.Load<object>(entityId) == null &&
					stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			WaitForStoreWithFunction(condition);
		}

		public void WaitForEntitiesToExist<T>(int numberOfEntities, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
				session.Query<T>().Count() < numberOfEntities &&
				stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			WaitForStoreWithFunction(condition);
		}
		protected void WaitForEntityToExist<T>(Func<T, bool> whereCondition, int count = 1, int secondsToWait = 30)
		{
			Func<IDocumentSession, Stopwatch, bool> condition =
				(session, stopwatch) =>
				session.Query<T>().Where(whereCondition).Count() < count &&
				stopwatch.ElapsedMilliseconds < secondsToWait * 1000;

			WaitForStoreWithFunction(condition);
		}


		public void WaitForStoreWithFunction(Func<IDocumentSession, Stopwatch, bool> condition)
		{
			var stopwatch = new Stopwatch();
			stopwatch.Start();

			using (var session = _documentStore.OpenSession())
			{
				session.Advanced.MaxNumberOfRequestsPerSession = 200;
				while (condition(session, stopwatch))
				{
					Thread.Sleep(500);
				}
			}
		}
	}
}
