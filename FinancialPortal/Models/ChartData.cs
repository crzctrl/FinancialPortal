using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FinancialPortal.Models
{
    public class BankAccountChartData
    {
        public string label { get; set; }
        public double value { get; set; }
        public string labels { get; set; }
    }

    public class BudgetChartData
    {
        public string label { get; set; }
        public double value { get; set; }
    }
}