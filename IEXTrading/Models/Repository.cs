using System;
using System.ComponentModel.DataAnnotations;

namespace IEXTrading.Models
{
    public class Repository
    {
        [Key]
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Type { get; set; }
        public float High { get; set; }
        public int Volume { get; set; }
        public float peRatio { get; set; }

        public Repository()
        { }

        public Repository(string symbol, string name, string date, string type, float high, int volume, float peratio)
        {
            Symbol = symbol;
            Name = name;
            Date = date;
            Type = type;
            High = high;
            Volume = volume;
            peRatio = peratio;
        }
    }
}
