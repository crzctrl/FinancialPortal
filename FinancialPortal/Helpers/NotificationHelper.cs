using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FinancialPortal.Models;

namespace FinancialPortal.Helpers
{
    public class NotificationHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public void SendNewRoleNotification(string userId, string role)
        {
            db.Notifications.Add(new Notification
            {
                Created = DateTime.Now,
                RecipientId = userId,
                Subject = $"You are now a {role}",
                Body = $"Relogin to start your new {role}",
                HouseholdId = (int)db.Users.Find(userId).HouseholdId
            });

            db.SaveChanges();
        }
    }
}