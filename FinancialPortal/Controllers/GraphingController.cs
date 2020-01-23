using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinancialPortal.Models;

namespace FinancialPortal.Controllers
{
    
    public class GraphingController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Graphing
        public JsonResult BankAccountChart(int houseId)
        {
            var myData = new List<BankAccountChartData>();
            BankAccountChartData data = null;
            foreach (var amount in db.BankAccounts.Where(b => b.HouseholdId == houseId).ToList())
            {
                data = new BankAccountChartData();
                data.label = amount.Name;
                data.value = amount.CurrentBalance;
                data.labels = amount.Name;
                myData.Add(data);
            }

            return Json(myData);
        }

        public JsonResult BudgetChart(int houseId)
        {
            var myData = new List<BudgetChartData>();
            BudgetChartData data = null;
            foreach (var amount in db.Budgets.Where(b => b.HouseholdId == houseId).ToList())
            {
                data = new BudgetChartData();
                data.label = amount.Name;
                data.value = amount.CurrentAmount;
                myData.Add(data);
            }

            return Json(myData);
        }
    }
}