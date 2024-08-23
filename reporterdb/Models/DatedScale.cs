using System;
using System.ComponentModel.DataAnnotations;

namespace nsreporter.ReporterDB.Models
{
    public class DatedScale
    {
        [Key]
        public int Id { get; set; }
        public int CensusId { get; set; }
        public DateTime CensusDate { get; set; }
        public double Score { get; set; }
        public int Rank { get; set; }
        public int RRank { get; set; }
    }
}