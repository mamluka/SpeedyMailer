namespace SpeedyMailer.Core.Evens
{
	public interface IHappendOn<in T> : IHappendOn
	{
		void Inspect(T data);
	}

	public interface IHappendOn
	{
	}
}