namespace SpeedyMailer.Tests.Core
{
    public interface IMockedComponentBuilder<out T>
    {
        T Build();
    }
}