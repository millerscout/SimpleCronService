using CronNET;
using MMDHelpers.CSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SimpleCronService
{
    public partial class Service1 : ServiceBase
    {
        private static readonly CronDaemon cron_daemon = new CronDaemon();
        private static bool EnableLog = false;

        public string PathToRun { get; private set; }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            var sched = ConfigurationManager.AppSettings["CronSchedule"];
            PathToRun = ConfigurationManager.AppSettings["PathToRun"];

            if (bool.TryParse(ConfigurationManager.AppSettings["EnableLog"], out var l)) EnableLog = l;

            sched = string.IsNullOrEmpty(sched) ? "*/30 * * * *" : sched;

            Log($"loaded Cron:{sched}");

            cron_daemon.AddJob(sched, Run);
            cron_daemon.Start();
        }

        private void Run()
        {
            Log($"started: {DateTime.Now:yyyy-MM-dd.HH.mm.ss}");
            if (bool.TryParse(ConfigurationManager.AppSettings["EnableLog"], out var l)) EnableLog = l;

            ProcessStartInfo psi;
            psi = new ProcessStartInfo
            {
                FileName = "steam://rungameid/233860",
                UseShellExecute = true
            };

            if (!File.Exists(PathToRun))
            {
                Log($"File Doesn't Exists: {PathToRun}");
                return;
            }
            psi = new ProcessStartInfo
            {
                FileName = PathToRun,
                WorkingDirectory = Path.GetDirectoryName(PathToRun)
            };
            try
            {


                var process = Process.Start(psi);
                Log($"stopped: {DateTime.Now:yyyy-MM-dd.HH.mm.ss}");
                process.Exited += Process_Exited;
            }
            catch (Exception ex)
            {

                Log($"Error: {ex.Message}");
                Log($"Error: {ex.StackTrace}");

            }

        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Log($"process completed {DateTime.Now:yyyy-MM-dd.HH.mm.ss}.");
        }

        private void Log(string log)
        {
            if (EnableLog)
            {
                "Log.txt".ToCurrentPath()
                .WriteToFile(new List<string> { log });
            }
        }

        protected override void OnStop()
        {
            cron_daemon.Stop();

        }
    }
}
