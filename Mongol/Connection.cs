/* Copyright 2012 Ephisys Inc.
Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
   limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver;
using System.Configuration;
using System.Threading;
using Common.Logging;

namespace Mongol
{
	/// <summary>
	/// This class coordinates the connection strings for Mongol.  By default it will read connection strings from the appSettings elements.  &quot;Mongol.Url&quot; is the key for the default connection.
	/// Specific named connections can be specified in appSettings as &quot;Mongol.Url.CONNECTIONNAME&quot;.  Alternatively, connections can be explicitly configured by the application by calling AddConnection.
	/// </summary>
	public static class Connection
	{
		private const string AppSettingPrefix = "Mongol.Url";

		private static readonly ILog Logger = LogManager.GetCurrentClassLogger();
		private static readonly Dictionary<string, MongoUrl> Connections;
		private static readonly ReaderWriterLockSlim Rwl = new ReaderWriterLockSlim();

		static Connection()
		{
			Connections = new Dictionary<string, MongoUrl>();
			foreach (var key in ConfigurationManager.AppSettings.Keys.Cast<string>().Where(key => key.StartsWith(AppSettingPrefix)))
			{
				if (key.Equals(AppSettingPrefix))
				{
					Logger.Debug(m => m("Initialized Mongol Connection:[default] - {0}", ConfigurationManager.AppSettings[key]));
					Connections.Add(String.Empty, new MongoUrl(ConfigurationManager.AppSettings[key]));
				}
				else
				{
					if (key.StartsWith(AppSettingPrefix + "."))
					{
						var connectionName = key.Substring(AppSettingPrefix.Length + 1);
						Logger.Debug(m => m("Initialized Mongol Connection:{0} - {1}", connectionName, ConfigurationManager.AppSettings[key]));
						Connections.Add(connectionName, new MongoUrl(ConfigurationManager.AppSettings[key]));
					}
				}
			}
		}

		/// <summary>
		/// Retrieves a MongoDatabase instance based upon the named connection.
		/// </summary>
		/// <param name="name">The name of the connection (null if default connection)</param>
		public static MongoDatabase GetInstance(string connectionString)
		{
			var mongoUrl = new MongoUrl(connectionString);
			return MongoServer.Create(mongoUrl).GetDatabase(mongoUrl.DatabaseName);
		}

		/// <summary>
		/// Sets the value for a named connection.
		/// </summary>
		/// <param name="name">The name of the new connection.</param>
		/// <param name="url">The MongoDB url for the connection.  Pass a value of null to remove the connection from the list.</param>
		public static void SetConnection(string name, string url)
		{
			Logger.Debug(m => m("SetConnection({0},{1})", name, url));
			Rwl.EnterWriteLock();
			try
			{
				if (String.IsNullOrEmpty(url) && Connections.ContainsKey(name))
				{
					Connections.Remove(name);
				}
				else
				{
					Connections[name ?? String.Empty] = new MongoUrl(url);
				}
			}
			finally
			{
				Rwl.ExitWriteLock();
			}
		}

		private static MongoUrl GetMongolUrlByName(string name)
		{
			Rwl.EnterReadLock();
			try
			{
				string suffix = String.IsNullOrEmpty(name) ? null : "." + name;
				if (!Connections.ContainsKey(name))
				{
					throw new ConfigurationErrorsException("Missing AppSetting Mongol.Url" + suffix);
				}
				return Connections[name];
			}
			finally
			{
				Rwl.ExitReadLock();
			}
		}
	}
}
