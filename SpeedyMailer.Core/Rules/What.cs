using System.Collections.Generic;

namespace SpeedyMailer.Core.Rules
{
	public class What
	{
		public WhatType Type { get; set; }
		public List<string> Conditions { get; set; }
	}
}