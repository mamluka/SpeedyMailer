namespace SpeedyMailer.Core.Apis
{
	public class ApiStringResult : ApiPrimitiveResult<string>
	{
		public override string Result { get; set; }
	}

	public abstract class ApiPrimitiveResult<T>
	{
		public abstract T Result { get; set; }
	}
}
