using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CommandLine;
using Microsoft.Web.Administration;

namespace SpeedyMailer.Master.Deploy
{
	class Program
	{
		public class DeployCommandOptions : CommandLineOptionsBase
		{
			[Option("s", "deploy-service")]
			public bool DeployService { get; set; }

			[Option("I", "deploy-api")]
			public bool DeployApi { get; set; }

			[Option("A", "deploy-app")]
			public bool DeployApp { get; set; }

			[Option("B", "base-directory")]
			public string BaseDirectory { get; set; }

			[Option("U", "base-url")]
			public string BaseUrl { get; set; }


		}

		private static void Main(string[] args)
		{
			var deployCommandOptions = new DeployCommandOptions();
			if (CommandLineParser.Default.ParseArguments(args, deployCommandOptions))
			{
				if (deployCommandOptions.DeployService)
				{
					DeployService(deployCommandOptions);
				}

				if (deployCommandOptions.DeployApi)
				{
					DeployApi(deployCommandOptions);
				}
				
				if (deployCommandOptions.DeployApp)
				{
					DeployApp(deployCommandOptions);
				}
			}
		}

		private static void DeployApp(DeployCommandOptions deployCommandOptions)
		{
			var iisManager = new ServerManager();
			const string name = "speedymailer.app";

			var exists = iisManager.Sites.Any(x => x.Name == name);

			var appPreReleasePath = Path.Combine(deployCommandOptions.BaseDirectory, "Release", "App");
			var appPath = Path.Combine(deployCommandOptions.BaseDirectory, "App");

			if (exists)
			{
				var site = iisManager.Sites[name];
				site.Stop();
				iisManager.CommitChanges();

				DeleteAppFolder(appPath, appPreReleasePath);

				site.Start();
				iisManager.CommitChanges();
			}
			else
			{
				DeleteAppFolder(appPath, appPreReleasePath);

				iisManager.Sites.Add(name, "http", string.Format("*:80:app.{0}", deployCommandOptions.BaseUrl), appPath);
				var site = iisManager.Sites[name];
				
				iisManager.CommitChanges();
			}
		}
		
		private static void DeployApi(DeployCommandOptions deployCommandOptions)
		{
			var iisManager = new ServerManager();
			const string name = "speedymailer.api";

			var exists = iisManager.Sites.Any(x => x.Name == name);

			var apiPreReleasePath = Path.Combine(deployCommandOptions.BaseDirectory, "Release", "Api");
			var apiPath = Path.Combine(deployCommandOptions.BaseDirectory, "Api");

			if (exists)
			{
				var site = iisManager.Sites[name];
				site.Stop();
				iisManager.CommitChanges();

				DeleteAppFolder(apiPath, apiPreReleasePath);

				site.Start();
				iisManager.CommitChanges();
			}
			else
			{
				DeleteAppFolder(apiPath, apiPreReleasePath);

				var hasPool = iisManager.ApplicationPools.Any(x => x.Name == name);
				if (!hasPool)
				{
					iisManager.ApplicationPools.Add(name);
					iisManager.CommitChanges();

					var pool = iisManager.ApplicationPools[name];
					pool.ManagedRuntimeVersion = "v4.0";

					iisManager.CommitChanges();
				}

				iisManager.Sites.Add(name, "http", string.Format("*:80:api.{0}", deployCommandOptions.BaseUrl), apiPath);
				var site = iisManager.Sites[name];

				var app = site.Applications["/"];
				app.ApplicationPoolName = name;

				iisManager.CommitChanges();				
			}
		}

		private static void DeleteAppFolder(string apiPath, string apiPreReleasePath)
		{
			if (Directory.Exists(apiPath))
				Directory.Delete(apiPath, true);

			Directory.Move(apiPreReleasePath, apiPath);
		}

		private static void DeployService(DeployCommandOptions deployCommandOptions)
		{
			var servicePreReleasePath = Path.Combine(deployCommandOptions.BaseDirectory, "Release", "Service");
			var serviceProcess = Process.GetProcessesByName("SpeedyMailer.Master.Service").FirstOrDefault();
			var servicePath = Path.Combine(deployCommandOptions.BaseDirectory, "Service");

			Console.WriteLine(servicePath);
			Console.WriteLine(serviceProcess);
			Console.WriteLine(servicePreReleasePath);

			if (serviceProcess != null)
			{
				serviceProcess.Kill();
				serviceProcess.WaitForExit(3000);
			}


			if (Directory.Exists(servicePath))
				Directory.Delete(servicePath, true);

			Directory.Move(servicePreReleasePath, servicePath);

			var fileName = Path.Combine(servicePath, "SpeedyMailer.Master.Service.exe");
			Console.WriteLine(fileName);

			var process = new Process
				{
					StartInfo =
						{
							FileName = fileName,
							Arguments = string.Format("--base-url {0}", string.Format("http://www.{0}", deployCommandOptions.BaseUrl))
						},
					EnableRaisingEvents = true
				};
			process.Start();
		}
	}
}
