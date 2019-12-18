using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinancialPortal.Models
{
    public class BankAccount
    {
        public int Id { get; set; }
        public int HouseholdId { get; set; }
        public string OwnerId { get; set; }

        public DateTime Created { get; set; }
        public string Name { get; set; }
        public AccountType AccountType { get; set; }

        public double StartingBalance { get; set; }
        public double CurrentBalance { get; set; }
        public double LowBalanceThreshold { get; set; }

        public virtual Household Household { get; set; }
        public virtual ApplicationUser Owner { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; }

        public BankAccount()
        {
            Transactions = new HashSet<Transaction>();
        }
    }
}