using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NationStatesAPI.Commands;
using NationStatesAPI.Models;
using nsreporter.Models;
using nsreporter.ReporterDB;
using nsreporter.ReporterDB.Models;

namespace nsreporter
{
    public class Reporter
    {
        public bool MustImportAll()
        {
            using (ReporterContext rctx = new ReporterContext())
            {
                return !rctx.DatedScales.Any();
            }
        }

        public CompareResult CompareOldData()
        {
            var toExclude = new List<CensusEnum>() { CensusEnum.WorldAssemblyEndorsements, CensusEnum.Survivors, CensusEnum.Population, CensusEnum.Zombies, CensusEnum.Dead };
            DateTime oggi = new DateTime(2020,1,20); //DateTime.Today;
            DateTime meseScorso = oggi.AddMonths(-1);
            using (ReporterContext rctx = new ReporterContext())
            {
                var datiOdierni = rctx.DatedScales
                                      .Where(x => !toExclude.Contains((CensusEnum)x.CensusId))
                                      .Where(x => x.CensusDate == oggi)
                                      .ToList();
                var datiVecchi = rctx.DatedScales
                                      .Where(x => !toExclude.Contains((CensusEnum)x.CensusId))
                                      .Where(x => x.CensusDate == meseScorso)
                                      .ToList();
                datiVecchi = datiVecchi.Where(x => x.CensusDate.Date == meseScorso).ToList();
                var confronto = (from tdy in datiOdierni
                                 join old in datiVecchi on tdy.CensusId equals old.CensusId
                                 select new ComparisonDTO()
                                 {
                                     CensusID = tdy.CensusId,
                                     NewScore = tdy.Score,
                                     OldScore = old.Score
                                 }).ToList();

                //removes stable to 0 data because no change happened and it's not worth mentioning
                confronto = confronto.Except(confronto.Where(x => x.NewScore == x.OldScore && x.NewScore == 0)).ToList();
                return new CompareResult()
                {
                    Improved = confronto.OrderByDescending(x => x.PercentageChange).Take(10).ToList(),
                    Worsened = confronto.OrderBy(x => x.PercentageChange).Take(10).ToList(),
                    ImprovedAbs = confronto.OrderByDescending(x => x.AbsoluteChange).Take(10).ToList(),
                    WorsenedAbs = confronto.OrderBy(x => x.AbsoluteChange).Take(10).ToList(),
                    Best = confronto.OrderByDescending(x => x.NewScore).Take(10).ToList(),
                    Worst = confronto.OrderBy(x => x.NewScore).Take(10).ToList()
                };
            }
        }

        public async Task GetLatestData(string nation)
        {
            using (ReporterContext rctx = new ReporterContext())
            {
                DateTime runningDate = DateTime.Today;

                // If today has already run and imported data, there is no point in doing this again
                if (!rctx.DatedScales.Where(x => x.CensusDate == runningDate).Any())
                {
                    NationCommands api = new NationCommands();
                    var censusData = await api.GetNationCensus(nation, CensusEnum.All);

                    foreach (var cens in censusData.CENSUS.Scale)
                    {
                        rctx.DatedScales.Add(new DatedScale()
                        {
                            CensusId = cens.Id,
                            CensusDate = runningDate,
                            Score = cens.Score,
                            Rank = cens.Rank,
                            RRank = cens.RRank
                        });
                    }
                    rctx.SaveChanges();
                }
            }
        }

        public async Task GetOldData(string nation)
        {
            NationCommands api = new NationCommands();
            Nation censusData = await api.GetHistoryNationCensus(nation, null, null, CensusEnum.All);

            using (ReporterContext rctx = new ReporterContext())
            {
                rctx.Database.EnsureCreated();

                foreach (var cens in censusData.CENSUS.Scale)
                {
                    foreach (var point in cens.Point)
                    {
                        rctx.DatedScales.Add(new DatedScale()
                        {
                            CensusId = cens.Id,
                            CensusDate = Helpers.FromUnixTime(point.TIMESTAMP).ToLocalTime().Date,
                            Score = Convert.ToDouble(point.SCORE),
                            Rank = 0,
                            RRank = 0
                        });
                    }

                    // Save each stat
                    rctx.SaveChanges();
                }

            }
        }

        public (string title, string report) ApplyComparisonToTemplate(CompareResult compareSets, string templatePath)
        {
            List<string> fileLines = File.ReadAllLines(templatePath, Encoding.Default).ToList();
            string title = fileLines.First();
            fileLines.RemoveAt(0);
            string template = String.Join(Environment.NewLine, fileLines);
            
            template = template.Replace("%IMPROVED%", MakeReportBody(compareSets.Improved));
            template = template.Replace("%WORSENED%", MakeReportBody(compareSets.Worsened));
            template = template.Replace("%IMPROVED_ABS%", MakeReportBody(compareSets.ImprovedAbs));
            template = template.Replace("%WORSENED_ABS%", MakeReportBody(compareSets.WorsenedAbs));
            template = template.Replace("%BEST%", MakeReportBody(compareSets.Best));
            template = template.Replace("%WORST%", MakeReportBody(compareSets.Worst));

            return (title: title, report: template);
        }

        private string MakeReportBody(List<ComparisonDTO> toPrintList)
        {
            StringBuilder forReplacement = new StringBuilder();

            string tmplChange = "[tr][td]{0}[/td][td]{1:0.00}[/td][td]{2:0.00}[/td][td]{3:0.00}[/td][td]{4:0.00}[/td][/tr]";

            foreach (var item in toPrintList)
            {
                var perc = Math.Round(item.PercentageChange, 2);
                forReplacement.AppendFormat(tmplChange, ((CensusEnum)item.CensusID), item.OldScore, item.NewScore, item.AbsoluteChange, perc);
                forReplacement.AppendLine();
            }

            return forReplacement.ToString();
        }
    }
}