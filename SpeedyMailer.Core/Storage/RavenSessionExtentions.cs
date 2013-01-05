using Raven.Client;

namespace SpeedyMailer.Core.Storage
{
	public static class RavenSessionExtentions
	{
		public static T LoadByType<T>(this IDocumentSession target)
		{
			return target.Load<T>(typeof (T).FullName);
		}
	}
}
