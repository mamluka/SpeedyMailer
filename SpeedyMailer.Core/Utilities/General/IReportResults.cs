namespace SpeedyMailer.Core.Utilities.General
{
    public interface IReportResults<out T>
    {
        T Results { get; }
    }
}