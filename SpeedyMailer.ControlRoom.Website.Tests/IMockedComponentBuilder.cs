namespace SpeedyMailer.ControlRoom.Website.Tests
{
    public  interface IMockedComponentBuilder<out T>
    {
        T Build();
    }
}