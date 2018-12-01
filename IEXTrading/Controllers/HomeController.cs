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
            List<Company> companies = JsonConvert.DeserializeObject<List<Company>>(TempData["Companies"].ToString());
            foreach (Company company in companies)
            {
                //Database will give PK constraint violation error when trying to insert record with existing PK.
                //So add company only if it doesnt exist, check existence using symbol (PK)
                if (dbContext.Companies.Where(c => c.symbol.Equals(company.symbol)).Count() == 0)
                {
                    dbContext.Companies.Add(company);
                }
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





        //Update the companies
        public IActionResult UpdateStocks() 
        {
            //Set ViewBag variable first
            ViewBag.dbSucessComp = 0;
            IEXHandler webHandler = new IEXHandler();
            List<Company> companies = webHandler.GetSymbols();

            //Save comapnies in TempData
            TempData["Companies"] = JsonConvert.SerializeObject(companies);
            List<Equity> equities = new List<Equity>();
            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);
            return View("Symbols", companiesEquities);
        }

        public IActionResult Reflection()
        {
            return View();
        }

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

            CompaniesEquities companiesEquities = getCompaniesEquitiesModel(equities);
            return View("Chart", companiesEquities);
        }

        public IActionResult MyRepository(string recordToDel)
        {
            //TODO: CLEAR RECORD
            ClearRecord(recordToDel);
            //the data post to page
            List<Repository> repositories = dbContext.Repositories.ToList();
            return View(repositories);
        }

        public IActionResult StockRecommendation()
        {
            dbContext.Repositories.RemoveRange(dbContext.Repositories);
            //TODO: STOCK PICKING STRATEGY:
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

            foreach (Financial finance in financials)
            {
                foreach (Company company in companies)
                {
                    if (finance.symbol == company.symbol)
                    {
                        IEXHandler webHandler = new IEXHandler();
                        List<Equity> equities = webHandler.GetChart(company.symbol);
                        string Name = company.name;
                        string Type = company.type;
                        string Date = equities.Last().date;
                        float AvgPrice = equities.Average(e => e.high);
                        int Volume = equities.Last().volume;
                        float peRatio = company.peRatio;

                        Repository repository = new Repository(company.symbol, Name, Date, Type, AvgPrice, Volume, peRatio);
                        dbContext.Repositories.Add(repository);
                    }
                }
            }
            dbContext.SaveChanges();

            return View("MyRepository", dbContext.Repositories.ToList());
        }

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
    }
}
