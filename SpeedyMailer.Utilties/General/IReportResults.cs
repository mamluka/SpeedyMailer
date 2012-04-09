namespace SpeedyMailer.Utilties.General
{
    public interface IReportResults<out T>
    {
        T Results { get; }
    }
}