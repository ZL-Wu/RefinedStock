using System;
using System.Collections.Generic;

namespace IEXTrading.Models
{
    public class Recommendation
    {
        List<Company> Companies { get; set; }
        Dictionary<string, float[]> Chart { get; set; }
        Dictionary<string, string[]> Date { get; set; }

        public Recommendation(List<Company> companies, Dictionary<string, float[]> chart, Dictionary<string, string[]> date)
        {
            Companies = companies;
            Chart = chart;
            Date = date;
        }
    }
}
