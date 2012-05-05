using System;
using Nancy;
using Ninject;
using SpeedyMailer.Core.Commands;

namespace SpeedyMailer.Master.Service.Api
{
    public abstract class ModuleBase:NancyModule
    {
        protected ModuleBase(string url):base(url)
        {
        }

        public void ExecuteCommand<T>(T command) where T : Command
        {
            command.Execute();
        }

        public void ExecuteCommand<T>(T command,Action<T> action) where T : Command
        {
            action.Invoke(command);
            command.Execute();
        }

        public TResult ExecuteCommand<T, TResult>(T command,Action<T> action) where T : Command<TResult>
        {
            action.Invoke(command);
            return command.Execute();
        }
    }
}