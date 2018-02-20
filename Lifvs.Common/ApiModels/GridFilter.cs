using System;
using System.Collections.Generic;

namespace Lifvs.Common.ApiModels
{
    public class GridFilter
    {
    }
    [Serializable]
    public class Filters
    {
        public string field { get; set; }
        public string op { get; set; }
        public string data { get; set; }
    }
    [Serializable]
    public class GetFilters
    {
        public string groupOp { get; set; }
        public List<Filters> rules { get; set; }
    }
}
