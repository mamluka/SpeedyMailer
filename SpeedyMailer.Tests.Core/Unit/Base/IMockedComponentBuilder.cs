namespace SpeedyMailer.Tests.Core.Unit.Base
{
    public interface IMockedComponentBuilder<out T>
    {
        T Build();
    }
}