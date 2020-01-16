namespace nsreporter.Models
{
    public class ComparisonDTO
    {
        public ComparisonDTO()
        {
        }

        public int CensusID { get; set; }
        public double NewScore { get; set; }
        public double OldScore { get; set; }

        public double PercentageChange
        {
            get
            {
                return ((NewScore - OldScore)/OldScore)*100;
            }
        }

        public double AbsoluteChange
        {
            get
            {
                return NewScore - OldScore;
            }
        }
    }
}