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
        static void ConsoleLog(string message, bool verbose)
        {
            if (verbose)
            {
                Console.WriteLine(message);
            }
        }
        static async Task Main(string[] args)
        {
            bool isVerbose = args?.Contains("-v") ?? false;
            ConsoleLog("NationStates Reporter v. 0.1.5.240823", isVerbose);

            IConfiguration Configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            AppSettings appSettings = new AppSettings();
            Configuration.Bind(appSettings);
            if (appSettings.MakeProgressReport && appSettings.DayOfMonth > 28)
            {
                throw new ArgumentException("Invalid day of month specified. Only all-months days are available (1-28)");
            }

            ConsoleLog("Configurazione caricata e valida", isVerbose);

            Reporter reporter = new Reporter();

            if (reporter.MustImportAll())
            {
                ConsoleLog("Caricamento completo dati", isVerbose);
                await reporter.GetOldData(appSettings.Nation);
            }
            else
            {
                DateTime lastCensus = reporter.GetLastDate().Date;
                if ((DateTime.Today - lastCensus).TotalDays > 1)
                {
                    ConsoleLog("Caricamento parziale dati", isVerbose);
                    await reporter.GetOldData(appSettings.Nation, lastCensus.AddDays(1), null);
                }
                else
                {
                    ConsoleLog("Caricamento ristretto dati", isVerbose);
                    await reporter.GetLatestData(appSettings.Nation);
                }
            }
            DateTime oggi = DateTime.Today;
            string dateArg = args.FirstOrDefault(a => a.StartsWith("-date:"));

            if (dateArg != null)
            {
                ConsoleLog($"Trovato parametro data: {dateArg}", isVerbose);

                dateArg = dateArg.Split(':').Last();
                if (!DateTime.TryParse(dateArg, out oggi))
                {
                    oggi = DateTime.Today;
                }
            }
            ConsoleLog($"La data inserita è: {oggi}", isVerbose);

            if (appSettings.MakeProgressReport && oggi.Day == appSettings.DayOfMonth)
            {
                ConsoleLog("Data valida per la generazione del report", isVerbose);

                var oldData = reporter.CompareOldData(oggi);
                (string title, string report) = reporter.ApplyComparisonToTemplate(oldData, appSettings.TemplateFile);
                DateTime meseScorso = oggi.AddMonths(-1);
                report = report.Replace("%LAST_MONTH%", meseScorso.ToShortDateString());
                report = report.Replace("%TODAY%", oggi.ToShortDateString());
                report = report.Replace("%NATION_NAME%", appSettings.Nation);

                title = title.Replace("%LAST_MONTH%", meseScorso.ToShortDateString());
                title = title.Replace("%TODAY%", oggi.ToShortDateString());

                ConsoleLog("Report compilato", isVerbose);
                ConsoleLog(report, isVerbose);


                NSCredentials nsCred = new NSCredentials(appSettings.Nation, appSettings.Password);
                await nsCred.Authenticate();
                var cmd = new PrivateCommands(nsCred);
                await cmd.CreateDispatch(Bulletin.DispatchType, (int)Bulletin.BulletinType.News, title, report);
                ConsoleLog("Report inviato con successo", isVerbose);

            }
        }
    }
}
