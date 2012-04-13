using Ninject;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    public class Actions : ActionsBase
    {
        public Actions(IKernel kernel):base(kernel)
        {}
    }
}