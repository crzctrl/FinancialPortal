using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using FinancialPortal.Models;
using Microsoft.AspNet.Identity;

namespace FinancialPortal.Controllers
{
    [Authorize]
    [RequireHttps]
    public class TransactionsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Transactions
        public ActionResult Index()
        {
            var transactions = db.Transactions.Include(t => t.BankAccount).Include(t => t.BudgetItem);
            return View(transactions.ToList());
        }

        // GET: Transactions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // GET: Transactions/Create
        public ActionResult Create()
        {
            var userHouseId = db.Users.Find(User.Identity.GetUserId()).HouseholdId;
            var userBdgtId = db.Budgets.Where(b => b.HouseholdId == userHouseId).FirstOrDefault().Id;
            ViewBag.BankAccountId = new SelectList(db.BankAccounts.Where(b => b.HouseholdId == userHouseId), "Id", "Name");
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems.Where(b => b.BudgetId == userBdgtId), "Id", "Name");
            return View();
        }

        // POST: Transactions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,BankAccountId,BudgetItemId,TransactionTypeId,Amount,Memo")] Transaction transaction)
        {
            var bnkAcct = db.BankAccounts.Find(transaction.BankAccountId);
            var bItem = db.BudgetItems.Find(transaction.BudgetItemId);
            var budget = db.Budgets.FirstOrDefault(b => b.Id == bItem.BudgetId);
            var ppl = db.Users.Find(User.Identity.GetUserId());
            
            if (ModelState.IsValid)
            {
                transaction.OwnerId = User.Identity.GetUserId();
                transaction.Created = DateTime.Now;
                db.Transactions.Add(transaction);

                switch (transaction.TransactionTypeId)
                {
                    case TransactionType.Deposit:
                        bnkAcct.CurrentBalance += transaction.Amount;
                        break;
                    case TransactionType.Withdrawal:
                        bnkAcct.CurrentBalance -= transaction.Amount;
                        bItem.CurrentAmount += transaction.Amount;
                        budget.CurrentAmount += transaction.Amount;
                        break;
                }
                // possible helper that takes a float
                //tHelp.Calulate(transaction.Amount);
                db.SaveChanges();

                if (bnkAcct.CurrentBalance <= bnkAcct.LowBalanceThreshold)
                {
                    try
                    {
                        EmailService ems = new EmailService();
                        IdentityMessage msg = new IdentityMessage();

                        msg.Body = $"Your account: {bnkAcct.Name} is running low on the funds!";
                        msg.Destination = ppl.Email;
                        msg.Subject = "HOLY MOLY";

                        await ems.SendMailAsync(msg);
                    }
                    catch
                    {
                        await Task.FromResult(0);
                    }
                }

                return RedirectToAction("Index");
            }

            //ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "OwnerId", transaction.BankAccountId);
            //ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            return View(transaction);
        }

        // GET: Transactions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "OwnerId", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            return View(transaction);
        }

        // POST: Transactions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,BankAccountId,BudgetItemId,TransactionTypeId,Created,Amount,Memo")] Transaction transaction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(transaction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BankAccountId = new SelectList(db.BankAccounts, "Id", "OwnerId", transaction.BankAccountId);
            ViewBag.BudgetItemId = new SelectList(db.BudgetItems, "Id", "Name", transaction.BudgetItemId);
            return View(transaction);
        }

        // GET: Transactions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Transaction transaction = db.Transactions.Find(id);
            if (transaction == null)
            {
                return HttpNotFound();
            }
            return View(transaction);
        }

        // POST: Transactions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Transaction transaction = db.Transactions.Find(id);
            var bnkAcct = db.BankAccounts.Find(transaction.BankAccountId);
            var bItem = db.BudgetItems.Find(transaction.BudgetItemId);
            var budget = db.Budgets.FirstOrDefault(b => b.Id == bItem.BudgetId);

            switch (transaction.TransactionTypeId)
            {
                case TransactionType.Deposit:
                    bnkAcct.CurrentBalance -= transaction.Amount;
                    break;
                case TransactionType.Withdrawal:
                    bnkAcct.CurrentBalance += transaction.Amount;
                    bItem.CurrentAmount -= transaction.Amount;
                    budget.CurrentAmount -= transaction.Amount;
                    break;
            }
            db.Transactions.Remove(transaction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
