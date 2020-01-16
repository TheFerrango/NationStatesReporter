using System;
using System.Collections.Generic;
using System.Linq;
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
        public CompareResult CompareOldData()
        {
            var toExclude = new List<CensusEnum>() { CensusEnum.WorldAssemblyEndorsements, CensusEnum.Survivors, CensusEnum.Population, CensusEnum.Zombies, CensusEnum.Dead };
            DateTime oggi = DateTime.Today;
            DateTime meseScorso = oggi.AddMonths(-1);
            using (ReporterContext rctx = new ReporterContext())
            {
                var datiOdierni = rctx.DatedScales
                                      .Where(x => !toExclude.Contains((CensusEnum)x.CensusId))
                                      .Where(x => x.CensusDate == oggi)
                                      .ToList();
                var datiVecchi = rctx.DatedScales
                                      .Where(x => !toExclude.Contains((CensusEnum)x.CensusId))
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

        // public void ApplyComparisonToTemplate(CompareResult compareResult, string templateFile)
        // {   
        //     Console.WriteLine("I 10 miglioramenti maggiori (%) sono:");
        //         string tmplChange = "[tr][td]{0}[/td][td]{1:0.00}[/td][td]{2:0.00}[/td][td]{3:0.00}[/td][td]{4:0.00}[/td][/tr]";

        //         foreach (var item in improved)
        //         {
        //             var perc = Math.Round(item.PercentageChange, 2);
        //             Console.WriteLine(tmplChange, ((CensusEnum)item.CensusID), item.OldScore, item.NewScore, item.AbsoluteChange, perc);
        //         }

        //         Console.WriteLine("I 10 peggioramenti maggiori (%) sono:");
        //         foreach (var item in worsened)
        //         {
        //             var perc = Math.Round(item.PercentageChange, 2);
        //             Console.WriteLine(tmplChange, ((CensusEnum)item.CensusID), item.OldScore, item.NewScore, item.AbsoluteChange, perc);
        //         }

        //         Console.WriteLine("I 10 miglioramenti maggiori (abs) sono:");
        //         foreach (var item in improvedAbs)
        //         {
        //             var perc = Math.Round(item.PercentageChange, 2);
        //             Console.WriteLine(tmplChange, ((CensusEnum)item.CensusID), item.OldScore, item.NewScore, item.AbsoluteChange, perc);
        //         }

        //         Console.WriteLine("I 10 peggioramenti maggiori (abs) sono:");
        //         foreach (var item in worsenedAbs)
        //         {
        //             var perc = Math.Round(item.PercentageChange, 2);
        //             Console.WriteLine(tmplChange, ((CensusEnum)item.CensusID), item.OldScore, item.NewScore, item.AbsoluteChange, perc);
        //         }

        //         Console.WriteLine("I 10 migliori sono:");
        //         foreach (var item in best)
        //         {
        //             var perc = Math.Round(item.PercentageChange, 2);
        //             Console.WriteLine(tmplChange, ((CensusEnum)item.CensusID), item.OldScore, item.NewScore, item.AbsoluteChange, perc);

        //         }

        //         Console.WriteLine("I 10 peggiori sono:");
        //         foreach (var item in worst)
        //         {
        //             var perc = Math.Round(item.PercentageChange, 2);
        //             Console.WriteLine(tmplChange, ((CensusEnum)item.CensusID), item.OldScore, item.NewScore, item.AbsoluteChange, perc);
        //         }
        // }   

        public async Task GetLatestData()
        {
            NationCommands api = new NationCommands();
            var censusData = await api.GetNationCensus("testlandia", CensusEnum.All);


            using (ReporterContext rctx = new ReporterContext())
            {
                rctx.Database.EnsureCreated();
                DateTime runningDate = DateTime.Today;
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
}