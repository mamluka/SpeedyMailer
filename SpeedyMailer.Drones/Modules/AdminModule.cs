using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Nancy;
using Quartz;
using SpeedyMailer.Core.Domain.Creative;
using SpeedyMailer.Core.Domain.Mail;
using SpeedyMailer.Core.Tasks;
using SpeedyMailer.Drones.Commands;
using SpeedyMailer.Drones.Storage;

namespace SpeedyMailer.Drones.Modules
{
    public class AdminModule : NancyModule
    {
        public AdminModule(IScheduler scheduler, LogsStore logsStore, ParseLogsCommand parseLogsCommand)
            : base("/admin")
        {

            Get["/hello"] = x => Response.AsText("OK");

            Get["/fire-task/{job}"] = x =>
                                                    {
                                                        scheduler.TriggerTaskByClassName((string)x.job);
                                                        return Response.AsText("OK");
                                                    };

            Get["/shutdown"] = x =>
                                   {
                                       var jobs = scheduler.GetCurrentJobs();
                                       scheduler.DeleteJobs(jobs);

                                       scheduler.Shutdown();

                                       while (!scheduler.IsShutdown)
                                       {
                                           Thread.Sleep(500);
                                       }

                                       Environment.Exit(0);

                                       return Response.AsText("OK");
                                   };

            Get["/raw-postfix-logs"] = x => Response.AsJson(logsStore.GetAllLogs());

            Get["/postfix-logs"] = x =>
                                       {
                                           var logs = logsStore.GetAllLogs();
                                           var lines = logs.Select(entry => string.Format("{0} {1}", entry.time.ToLongTimeString(), entry.msg)).ToList();
                                           return Response.AsText(string.Join(Environment.NewLine, lines));
                                       };

            Get["/write-headers/{header}/{value}"] = x =>
                {
                    var logs = logsStore.GetUnprocessedLogs();
                    parseLogsCommand.Logs = logs;
                    var parsedLogs = parseLogsCommand.Execute();


                    var fakeLogs = parsedLogs.Select(
                        mailEvent =>
                        {
                            var header = (string)x.header;
                            var value = (string)x.value;
                            return new MailLogEntry
                                {
                                    msg = mailEvent.MessageId + ": " + header + ": " + value + " ",
                                    level = "INFO",
                                    pid = "1235",
                                    procid = "postfix",
                                    sys = "mail",
                                    syslog_fac = 2,
                                    syslog_sever = 6,
                                    syslog_tag = "postfix/smtpd[27723]:",
                                    time = DateTime.UtcNow.AddDays(-10),
                                    time_rcvd = DateTime.UtcNow.AddDays(-10)
                                };
                        }
                        ).ToList();

                    logsStore.BatchInsert(fakeLogs);

                    return Response.AsJson(fakeLogs);
                };

        }
    }
}
