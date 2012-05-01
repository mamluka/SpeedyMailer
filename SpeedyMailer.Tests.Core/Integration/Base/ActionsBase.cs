using System;
using Ninject;
using SpeedyMailer.Core.Commands;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    public class ActionsBase
    {
        private readonly IKernel _kernel;

        public ActionsBase(IKernel kernel)
        {
            _kernel = kernel;
        }

        public void ExecuteCommand<T>(Action<T> action) where T:Command
        {
            var command = _kernel.Get<T>();
            action.Invoke(command);
            command.Execute();
        }

        public TResult ExecuteCommand<T,TResult>(Action<T> action) where T:Command<TResult>
        {
            var command = _kernel.Get<T>();
            action.Invoke(command);
            return command.Execute();
        }
    }
}