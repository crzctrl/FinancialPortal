using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FinancialPortal.Models;
using FinancialPortal.Helpers;
using Microsoft.AspNet.Identity;

namespace FinancialPortal.Controllers
{
    [Authorize]
    [RequireHttps]
    public class HomeController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TransactionHelper tHelp = new TransactionHelper();
        public ActionResult Dashboard()
        {
            var myHsId = db.Users.Find(User.Identity.GetUserId()).HouseholdId;
            if (myHsId == null)
            {
                return View();
            }

            return View(tHelp.ListHouseholdTransactions());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}