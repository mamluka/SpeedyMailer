namespace SpeedyMailer.Master.Web.Core.Builders
{
    public interface IViewModelBuilder<T> where T : new()
    {
        T Build();
    }
}