using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IEXTrading.Infrastructure.IEXTradingHandler;
using IEXTrading.Models;
using IEXTrading.Models.ViewModel;
using IEXTrading.DataAccess;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Diagnostics.Contracts;

namespace MVCTemplate.Controllers
{
    public class HomeController : Controller
    {
        public ApplicationDbContext dbContext;

        public HomeController(ApplicationDbContext context)
        {
            dbContext = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        /****
         * The Symbols action calls the GetSymbols method that returns a list of Companies.
         * This list of Companies is passed to the Symbols View.
        ****/
        public IActionResult Symbols()
        {
            List<Company> companies = dbContext.Companies.ToList();
            List<Equity> equities = new List<Equity>();
            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);
            return View(companiesEquities);
        }

        /****
         * The Chart action calls the GetChart method that returns 1 year's equities for the passed symbol.
         * A ViewModel CompaniesEquities containing the list of companies, prices, volumes, avg price and volume.
         * This ViewModel is passed to the Chart view.
        ****/
        public IActionResult Chart(string symbol)
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessChart = 0;
            ViewBag.dbSuccessRep = 0;
            List<Equity> equities = new List<Equity>();
            if (symbol != null)
            {
                IEXHandler webHandler = new IEXHandler();
                equities = webHandler.GetChart(symbol);
                equities = equities.OrderBy(c => c.date).ToList(); //Make sure the data is in ascending order of date.
            }

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);

            return View(companiesEquities);
        }

        /****
         * The Refresh action calls the ClearTables method to delete records from a or all tables.
         * Count of current records for each table is passed to the Refresh View.
        ****/
        public IActionResult Refresh(string tableToDel)
        {
            ClearTables(tableToDel);
            Dictionary<string, int> tableCount = new Dictionary<string, int>
            {
                { "Companies", dbContext.Companies.Count() },
                { "Charts", dbContext.Equities.Count() }
            };
            return View(tableCount);
        }

        /****
         * Saves the Symbols in database.
        ****/
        public IActionResult PopulateSymbols()
        {
            ClearTables("all");
            List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(TempData["Companies"].ToString());
            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                //if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                //{
                    dbContext.Companies.Add(company);
                //}
            }
            dbContext.SaveChanges();
            ViewBag.dbSuccessComp = 1;

            List<Equity> equities = new List<Equity>();
            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);
            return View("Symbols", companiesEquities);
        }

        /****
         * Saves the equities in database.
        ****/
        public IActionResult SaveCharts(string symbol)
        {
            IEXHandler webHandler = new IEXHandler();
            List<Equity> equities = webHandler.GetChart(symbol);
            //List<Equity> equities = JsonConvert.DeserializeObject<List<Equity>>(TempData["Equities"].ToString());
            foreach (Equity equity in equities)
            {
                if (dbContext.Equities.Where(c => c.date.Equals(equity.date)).Count() == 0)
                {
                    dbContext.Equities.Add(equity);
                }
            }

            dbContext.SaveChanges();
            ViewBag.dbSuccessChart = 1;

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);

            return View("Chart", companiesEquities);
        }

        /****
         * Deletes the records from tables.
        ****/
        public void ClearTables(string tableToDel)
        {
            if ("all".Equals(tableToDel))
            {
                //First remove equities and then the companies
                dbContext.Equities.RemoveRange(dbContext.Equities);
                dbContext.Companies.RemoveRange(dbContext.Companies);
            }
            else if ("Companies".Equals(tableToDel))
            {
                //Remove only those that don't have Equity stored in the Equitites table
                dbContext.Companies.RemoveRange(dbContext.Companies
                                                         .Where(c => c.Equities.Count == 0)
                                                                      );
            }
            else if ("Charts".Equals(tableToDel))
            {
                dbContext.Equities.RemoveRange(dbContext.Equities);
            }
            dbContext.SaveChanges();
        }

        /****
         * Returns the ViewModel CompaniesEquities based on the data provided.
         ****/
        public CompaniesEquities getCompaniesEquitiesModel(List<Equity> equities)
        {
            List<Company> companies = dbContext.Companies.ToList();

            if (equities.Count == 0)
            {
                return new CompaniesEquities(companies, null, "", "", "", 0, 0, "", "", "", "");
            }

            Equity current = equities.Last();
            string dates = string.Join(",", equities.Select(e => e.date));
            string prices = string.Join(",", equities.Select(e => e.high));
            string volumes = string.Join(",", equities.Select(e => e.volume / 1000000)); //Divide vol by million
            float avgprice = equities.Average(e => e.high);
            double avgvol = equities.Average(e => e.volume) / 1000000; //Divide volume by million
            string open = string.Join(",", equities.Select(e => e.open));
            string high = string.Join(",", equities.Select(e => e.high));
            string low = string.Join(",", equities.Select(e => e.low));
            string close = string.Join(",", equities.Select(e => e.close));
            return new CompaniesEquities(companies, equities.Last(), dates, prices, volumes, avgprice, avgvol, open, high, low, close);
        }




        //TODO: Unfinished!!!
        public Recommendation GetRecommendationModel(List<Company> companies)
        {
            List<string> symbols = new List<string>();
            List<Equity> equities = new List<Equity>();
            Dictionary<string, float[]> chart = new Dictionary<string, float[]>();
            Dictionary<string, string[]> date = new Dictionary<string, string[]>();
            foreach (Company company in companies)
                symbols.Add(company.symbol);
            foreach (string symbol in symbols)
            {
                IEXHandler webHandler = new IEXHandler();
                equities = webHandler.GetChart(symbol);
                equities = equities.OrderBy(c => c.date).ToList();

                //chart.Add(symbol, equities.)
            }

            return new Recommendation(companies, chart, date);
        }

        //Update the companies
        public IActionResult UpdateStocks() 
        {
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;
            IEXHandler webHandler = new IEXHandler();
            List<Company> companies = webHandler.GetSymbols();
            foreach (Company company in companies)
            {
                IEXHandler webHandler2 = new IEXHandler();
                Quote quote = webHandler2.GetQuote(company.symbol);
                if (quote.peRatio > 0)
                {
                    company.peRatio = quote.peRatio;
                }
                else
                {
                    company.peRatio = 10000;
                }
            }
            //Only pick out top 20 companies which has lower PE ratio
            companies = companies.OrderBy(c => c.peRatio).ToList().GetRange(0, 20);
            foreach(Company company in companies)
            {
                float BetaValue = GetBetaValue(company.symbol);
                company.BetaRisk = BetaValue;
            }

            //Save comapnies in TempData
            TempData["Companies"] = JsonConvert.SerializeObject(companies);

            CompaniesEquities companiesEquities = new CompaniesEquities(companies, null, "", "", "", 0, 0, "", "", "", "");
            return View("Symbols", companiesEquities);
        }

        public IActionResult Reflection()
        {
            return View();
        }
        public IActionResult Recommendation()
        {
            return View();
        }

        //clear my repository record
        public void ClearRecord(string recordToDel)
        {
            if (recordToDel != null)
            {
                dbContext.Remove(dbContext.Repositories.Single(r => r.Symbol.Equals(recordToDel)));
            }
            dbContext.SaveChanges();
        }

        public IActionResult AddRepository(string symbol)
        {
            Contract.Ensures(Contract.Result<IActionResult>() != null);
            ViewBag.dbSuccessRep = 1;
            IEXHandler webHandler = new IEXHandler();
            List<Equity> equities = webHandler.GetChart(symbol);
            List<Company> companies = dbContext.Companies.Where(c => c.symbol.Equals(symbol)).ToList();
            if (dbContext.Repositories.Where(r => r.Symbol.Equals(symbol)).Count() == 0)
            {
                string Name = companies[0].name;
                string Type = companies[0].type;
                string Date = equities.Last().date;
                float High = equities.Last().high;
                int Volume = equities.Last().volume;
                float peRatio = companies[0].peRatio;
                Repository repository = new Repository(symbol, Name, Date, Type, High, Volume, peRatio);
                dbContext.Repositories.Add(repository);
                dbContext.SaveChanges();
            }

            List<Repository> repositories = dbContext.Repositories.ToList();
            return View("MyRepository", repositories);
            //CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);
            //return View("Chart", companiesEquities);
        }

        public IActionResult MyRepository(string recordToDel)
        {
            //CLEAR RECORD
            ClearRecord(recordToDel);
            ViewBag.dbSuccessRep = 0;
            //the data post to page
            List<Repository> repositories = dbContext.Repositories.ToList();
            return View(repositories);
        }

        //Get the K line chart of a certain stock
        public IActionResult Details(string symbol)
        {
            //Set ViewBag variable first
            ViewBag.dbSuccessChart = 0;
            ViewBag.dbSuccessRep = 0;
            List<Equity> equities = new List<Equity>();
            if (symbol != null)
            {
                IEXHandler webHandler = new IEXHandler();
                equities = webHandler.GetChart(symbol);
                equities = equities.OrderBy(c => c.date).ToList(); //Make sure the data is in ascending order of date.
            }

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);
            return View("Symbols",companiesEquities);
        }

        //public IActionResult CompareCharts(string symbol)
        //{
        //    List<Equity> equities = new List<Equity>();
        //    if (symbol != null)
        //    {
        //        IEXHandler webHandler = new IEXHandler();
        //        equities = webHandler.GetChart(symbol);
        //        equities = equities.OrderBy(c => c.date).ToList(); //Make sure the data is in ascending order of date.
        //    }

        //    //TODO: TEST WILL DELETE LATER!
        //    //Equity current = equities.Last();
        //    //string dates = string.Join(",", equities.Select(e => e.date));
        //    //string prices = string.Join(",", equities.Select(e => e.high));
        //    //string volumes = string.Join(",", equities.Select(e => e.volume / 1000000)); //Divide vol by million
        //    //float avgprice = equities.Average(e => e.high);
        //    //double avgvol = equities.Average(e => e.volume) / 1000000; //Divide volume by million
        //    //string open = string.Join(",", equities.Select(e => e.open));
        //    //string high = string.Join(",", equities.Select(e => e.high));
        //    //string low = string.Join(",", equities.Select(e => e.low));
        //    //string close = string.Join(",", equities.Select(e => e.close));
        //    //CompaniesEquities companiesEquities = new CompaniesEquities(newCompanies, equities.Last(), dates, prices, volumes, avgprice, avgvol, open, high, low, close);

        //    return View("Recommendation", companiesEquities);
        //}

        public IActionResult StockRecommendation()
        {
            //dbContext.Repositories.RemoveRange(dbContext.Repositories);
            //STOCK PICKING STRATEGY:
            List<Company> companies = dbContext.Companies.ToList();
            List<Financial> financials = new List<Financial>();
            foreach (Company company in companies)
            {
                IEXHandler webHandler = new IEXHandler();
                Financial financial = webHandler.getFinancial(company.symbol);
                financials.Add(financial);
            }

            //Filt out good stocks depend on the financial report data
            financials = financials.OrderByDescending(f => f.operatingRevenue).ToList().GetRange(0, 15);
            financials = financials.OrderByDescending(f => f.totalAssets).ToList().GetRange(0, 10);
            financials = financials.OrderByDescending(f => f.cashFlow).ToList().GetRange(0, 5);

            List<Company> newCompanies = new List<Company>();
            foreach (Financial finance in financials)
            {
                foreach (Company company in companies)
                {
                    if (finance.symbol == company.symbol)
                        newCompanies.Add(company);
                }
            }

            CompaniesEquities companiesEquities = new CompaniesEquities(newCompanies, null, "", "", "", 0, 0, "", "", "", "");
            return View("Recommendation", companiesEquities);
        }

        //Second Recommendation algorithm (CAPM)
        public IActionResult CAPMrecommendation()
        {
            List<Company> Companies = dbContext.Companies.ToList();
            Companies = Companies.OrderBy(c => c.BetaRisk).ToList().GetRange(0, 5);
            CompaniesEquities companiesEquities = new CompaniesEquities(Companies, null, "", "", "", 0, 0, "", "", "", "");
            return View("Recommendation", companiesEquities);
        }

        //CAPM: Get Asset Return from a certain stock
        //Market Return from S&P 500 is when symbol equals to "SPY"
        public List<float> GetAssetReturns(string symbol)
        {
            //Get Stocks 1y data
            IEXHandler webHandler = new IEXHandler();
            List<Equity> equites = webHandler.GetChart(symbol);

            //Calulate market return based on each day
            List<float> assetReturns = new List<float>();
            foreach (Equity equity in equites)
            {
                float Areturn = (equity.close - equity.open) / equity.open;
                assetReturns.Add(Areturn);
            }
            return assetReturns;
        }

        //CAPM: Get Beta Value of a certain stock
        //Use linear regression model
        public float GetBetaValue(string symbol)
        {
            //Get Group Y: Estimated Stock Return 
            List<float> EstStockReturn = GetAssetReturns(symbol);
            //Get Group X: Estimated Market Return
            List<float> EstMarketReturn = GetAssetReturns("SPY");
            //Calculate Beta Value of this stock
            int N = EstStockReturn.Count();
            float XYSum = 0f;
            float XSum = 0f;
            float YSum = 0f;
            float X2Sum = 0f;

            for(int i = 0; i < N; i++)
            {
                XYSum += EstMarketReturn[i] * EstStockReturn[i];
                XSum += EstMarketReturn[i];
                YSum += EstStockReturn[i];
                X2Sum += EstMarketReturn[i] * EstMarketReturn[i];
            }

            float Beta = (N * XYSum - XSum * YSum) / (N * X2Sum - XSum * XSum);
            return Beta;
        }
    }
}
