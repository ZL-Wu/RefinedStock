using System;
namespace IEXTrading.Models
{
    public class QuoteRoot
    {
        public Quote[] Qroot;
    }

    public class Quote
    {
        public string symbol { get; set; }
        //public string companyName;
        //public string primaryExchange;
        //public string sector;
        //public string calculationPrice;
        //public float open;
        //public Int64 openTime;
        //public float close;
        //public Int64 closeTime;
        //public float high;
        //public float low;
        //public float latestPrice;
        //public string latestSource;
        //public string latestTime;
        //public int latestUpdate;
        //public int latestVolume;
        //public float iexRealtimePrice;
        //public int iexRealtimeSize;
        //public int iexLastUpdated;
        //public float delayedPrice;
        //public Int64 delayedPriceTime;
        //public float extendedPrice;
        //public float extendedChange;
        //public float extendedChangePercent;
        //public Int64 extendedPriceTime;
        //public float previousClose;
        //public float change;
        //public float changePercent;
        //public float iexMarketPercent;
        //public int iexVolume;
        //public int avgTotalVolume;
        //public float iexBidPrice;
        //public int iexBidSize;
        //public float iexAskPrice;
        //public int iexAskSize;
        //public int marketCap;
        public float peRatio { get; set; }
        //public float week52High;
        //public float week52Low;
        //public float ytdChange;
    }
}
