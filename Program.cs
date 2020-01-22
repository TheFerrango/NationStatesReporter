using System;
using NationStatesAPI.Commands;
using NationStatesAPI.Models;
using nsreporter.ReporterDB;
using nsreporter.ReporterDB.Models;
using System.Linq;
using System.Collections.Generic;
using nsreporter.Models;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using NationStatesAPI.Login;
using NationStatesAPI.Models.Dispatch;

namespace nsreporter
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            AppSettings appSettings = new AppSettings();
            Configuration.Bind(appSettings);
            if (appSettings.MakeProgressReport && appSettings.DayOfMonth > 28)
            {
                throw new ArgumentException("Invalid day of month specified. Only all-months days are available (1-28)");
            }

            Reporter reporter = new Reporter();

            if (reporter.MustImportAll())
            {
                await reporter.GetOldData(appSettings.Nation);
            }
            else
            {
                await reporter.GetLatestData(appSettings.Nation);
            }

            if (appSettings.MakeProgressReport && DateTime.Today.Day == appSettings.DayOfMonth)
            {
                var oldData = reporter.CompareOldData();
                (string title, string report) = reporter.ApplyComparisonToTemplate(oldData, appSettings.TemplateFile);
                DateTime oggi = DateTime.Today;
                DateTime meseScorso = oggi.AddMonths(-1);
                report = report.Replace("%LAST_MONTH%", meseScorso.ToShortDateString());
                report = report.Replace("%TODAY%", oggi.ToShortDateString());
                report = report.Replace("%NATION_NAME%", appSettings.Nation);

                title = title.Replace("%LAST_MONTH%", meseScorso.ToShortDateString());
                title = title.Replace("%TODAY%", oggi.ToShortDateString());

                NSCredentials nsCred = new NSCredentials(appSettings.Nation, appSettings.Password);
                await nsCred.Authenticate();
                var cmd = new PrivateCommands(nsCred);
                await cmd.CreateDispatch(Bulletin.DispatchType, (int)Bulletin.BulletinType.News, title, report);
            }
        }
    }
}
