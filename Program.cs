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
            if(appSettings.MakeProgressReport && appSettings.DayOfMonth > 28)
            {
                throw new ArgumentException("Invalid day of month specified. Only all-months days are available (1-28)");
            }

            Reporter reporter = new Reporter();
            await reporter.GetLatestData();

            
            // if(appSettings.MakeProgressReport && DateTime.Today.Day == appSettings.DayOfMonth )
            // {
            //     var oldData = reporter.CompareOldData();
            //     reporter.ApplyComparisonToTemplate(oldData, appSettings.TemplateFile);
            // }
        }

        
    }
}
