using System.Collections.Generic;
using nsreporter.Models;

namespace nsreporter
{
    public class CompareResult
    {
        public List<ComparisonDTO> Improved { get; set; }
        public List<ComparisonDTO> Worsened { get; set; }
        public List<ComparisonDTO> ImprovedAbs { get; set; }
        public List<ComparisonDTO> WorsenedAbs { get; set; }
        public List<ComparisonDTO> Best { get; set; }
        public List<ComparisonDTO> Worst { get; set; }
    }
}