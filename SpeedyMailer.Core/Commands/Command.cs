using System;
using SpeedyMailer.Core.Api;

namespace SpeedyMailer.Core.Commands
{
	public abstract class Command
	{
		public abstract void Execute();
	}

	public abstract class Command<T>
	{
		public abstract T Execute();
	}

	public abstract class ApiCommand : Command<ApiResult>
	{ }
}