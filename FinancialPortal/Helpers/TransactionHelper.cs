using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FinancialPortal.Models;
using Microsoft.AspNet.Identity;

namespace FinancialPortal.Helpers
{
    public class TransactionHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public List<Transaction> ListHouseholdTransactions()
        {
            var houseTickets = new List<Transaction>();
            var user = db.Users.Find(HttpContext.Current.User.Identity.GetUserId());
            houseTickets.AddRange(db.BankAccounts.Where(b => b.HouseholdId == user.HouseholdId).SelectMany(b => b.Transactions));

            return houseTickets;
        }
    }
}