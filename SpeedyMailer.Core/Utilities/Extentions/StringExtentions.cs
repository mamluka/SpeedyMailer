using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpeedyMailer.Core.Utilities.Extentions
{
	public static class StringExtentions
	{
		public static readonly string RavenCollectionSeperator = "/";

		public static string StripRavenId(this string target)
		{
			return target.Split('/')[1];
		}

		public static string BuildRavenId(this string target, string ravenCollectionName, int index)
		{
			return ravenCollectionName + RavenCollectionSeperator + target.Split(',')[index];
		}

		public static string GetDomain(this string target)
		{
			return Regex.Match(target, "@(.+?)$").Groups[1].Value;
		}

		public static bool IsEmpty(this string target)
		{
			return string.IsNullOrEmpty(target);
		}

		public static bool HasValue(this string target)
		{
			return !string.IsNullOrEmpty(target);
		}
	}
}
