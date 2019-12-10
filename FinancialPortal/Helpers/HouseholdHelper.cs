using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FinancialPortal.Models;
using Microsoft.AspNet.Identity;

namespace FinancialPortal.Helpers
{
    public class HouseholdHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        //public int myHouseholdId()
        //{
        //    var userNull = db.Users.Find(HttpContext.Current.User.Identity.GetUserId()).HouseholdId;
        //    int user = (int)userNull;

        //    return user;
        //}

        public ICollection<ApplicationUser> MembersOfHousehold(int? householdId)
        {
            return db.Households.Find(householdId).Members;
        }

    }
}