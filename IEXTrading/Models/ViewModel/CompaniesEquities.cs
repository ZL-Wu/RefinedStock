using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IEXTrading.Models.ViewModel
{
    public class CompaniesEquities
    {
        public List<Company> Companies { get; set; }
        public Equity Current { get; set; }
        public string Dates { get; set; }
        public string Prices { get; set; }
        public string Volumes { get; set; }
        public float AvgPrice { get; set; }
        public double AvgVolume { get; set; }

        public string Open { get; set; }
        public string High { get; set; }
        public string Low { get; set; }
        public string Close { get; set; }

        public CompaniesEquities(List<Company> companies, Equity current, string dates, string prices, string volumes, float avgprice, double avgvolume, string open, string high, string low, string close)
        {
            Companies = companies;
            Current = current;
            Dates = dates;
            Prices = prices;
            Volumes = volumes;
            AvgPrice = avgprice;
            AvgVolume = avgvolume;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }
    }
}
