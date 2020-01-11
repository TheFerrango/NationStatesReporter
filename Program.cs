using System;
using NationStatesAPI.Commands;
using NationStatesAPI.Models;
using nsreporter.ReporterDB;
using nsreporter.ReporterDB.Models;
using System.Linq;

namespace nsreporter
{
    class Program
    {
        static void Main(string[] args)
        {
        //    GetLatestData();
        CompareOldData();
        }

        private static void CompareOldData()
        {
            DateTime oggi = DateTime.Today;
            DateTime meseScorso = oggi.AddMonths(-1);
            using (ReporterContext rctx = new ReporterContext())
            {
                var datiOdierni = rctx.DatedScales.Where(x=> x.CensusDate == oggi).ToList();
                var datiVecchi = rctx.DatedScales.ToList();
                datiVecchi = datiVecchi.Where(x=> x.CensusDate.Date == meseScorso).ToList();
                var confronto = (from tdy in datiOdierni
                                join old in datiVecchi on tdy.CensusId equals old.CensusId
                                select new 
                                {
                                    CensusID = tdy.CensusId,
                                    NewScore = tdy.Score,
                                    OldScore = old.Score
                                }).ToList();

                var migliorati = confronto.OrderByDescending(x=>x.NewScore - x.OldScore).Take(10).ToList();
                var peggiorati = confronto.OrderBy(x=>x.NewScore - x.OldScore).Take(10).ToList();
                var migliori = confronto.OrderByDescending(x=>x.NewScore).Take(10).ToList();
                var peggiori = confronto.OrderBy(x=>x.NewScore).Take(10).ToList();

                Console.WriteLine("I 10 miglioramenti maggiori sono:");
                foreach (var item in migliorati)
                {
                    var perc = Math.Round(((item.NewScore-item.OldScore) / item.OldScore)*100,2);                    
                    Console.WriteLine($"{((CensusEnum)item.CensusID)}: {item.NewScore - item.OldScore} ({perc})");
                }

                Console.WriteLine("I 10 peggioramenti maggiori sono:");
                foreach (var item in peggiorati)
                {
                    var perc = Math.Round(((item.NewScore-item.OldScore) / item.OldScore)*100,2);                    
                    Console.WriteLine($"{((CensusEnum)item.CensusID)}: {item.NewScore - item.OldScore} ({perc})");
                }

                 Console.WriteLine("I 10 migliori sono:");
                foreach (var item in migliori)
                {
                    Console.WriteLine($"{((CensusEnum)item.CensusID)}: {item.NewScore}");
                }

                Console.WriteLine("I 10 peggiori sono:");
                foreach (var item in peggiori)
                {
                    Console.WriteLine($"{((CensusEnum)item.CensusID)}: {item.NewScore}");
                }
            }
        }

        private static void GetLatestData()
        {
            NationCommands api = new NationCommands();
            var censusData = api.GetNationCensus("testlandia", CensusEnum.All).Result;


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
