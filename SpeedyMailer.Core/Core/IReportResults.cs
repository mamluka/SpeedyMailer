namespace SpeedyMailer.Core.Core
{
    public interface IReportResults<out T>
    {
        T Results { get; }
    }
}