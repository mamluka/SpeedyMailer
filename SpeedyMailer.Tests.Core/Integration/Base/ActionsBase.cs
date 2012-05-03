using System;
using System.Linq.Expressions;
using Ninject;
using SpeedyMailer.Core.Commands;

namespace SpeedyMailer.Tests.Core.Integration.Base
{
    public abstract class ActionsBase
    {
    	public readonly IKernel Kernel;

    	protected ActionsBase(IKernel kernel)
        {
            Kernel = kernel;
        }

		public void ExecuteCommand<T>() where T : Command
		{
			var command = Kernel.Get<T>();
			command.Execute();
		}

        public void ExecuteCommand<T>(Action<T> action) where T:Command
        {
            var command = Kernel.Get<T>();
            action.Invoke(command);
            command.Execute();
        }

        public TResult ExecuteCommand<T,TResult>(Action<T> action) where T:Command<TResult>
        {
            var command = Kernel.Get<T>();
            action.Invoke(command);
            return command.Execute();
        }

    	public abstract void EditSettings<T>(Action<T> expression);
    }
}