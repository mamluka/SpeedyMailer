using Raven.Client;

namespace SpeedyMailer.Core.Storage
{
	public static class RavenSessionExtentions
	{
		public static T LoadSingle<T>(this IDocumentSession target)
		{
			return target.Load<T>(typeof(T).FullName);
		}

		public static void StoreSingle(this IDocumentSession target, IHasId entity)
		{
			entity.Id = entity.GetType().FullName;
			target.Store(entity);
		}
	}

	public interface IHasId
	{
		string Id { get; set; }
	}
}
