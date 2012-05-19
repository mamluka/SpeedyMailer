namespace SpeedyMailer.Core.Tasks
{
	public class ComputationResult<T>
	{
		public string Id { get; set; }
		public T Result { get; set; }
	}
}