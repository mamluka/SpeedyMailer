namespace SpeedyMailer.ControlRoom.Website.Tests
{
    public  interface IControllerBuilder<out T>
    {
        T Build();
    }
}