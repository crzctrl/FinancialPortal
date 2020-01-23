using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using FinancialPortal.Models;
using FinancialPortal.Helpers;
using FinancialPortal.Extensions;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;

namespace FinancialPortal.Controllers
{
    [Authorize]
    [RequireHttps]
    public class HouseholdsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private RoleHelper rHelp = new RoleHelper();
        private HouseholdHelper hHelp = new HouseholdHelper();
        private NotificationHelper nHelp = new NotificationHelper();

        // GET: Households
        [Authorize(Roles = "Head of Household")]
        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());
            return View(db.Households.Where(h => h.Id == user.HouseholdId).ToList());           
        }

        public ActionResult Members()
        {
            if (User.IsInRole("Admin"))
            {
                var allUsers = db.Users.ToList();
                return View(allUsers);
            }
            
            var houseId = db.Users.Find(User.Identity.GetUserId()).HouseholdId;
            var members = hHelp.MembersOfHousehold(houseId);

            return View(members);
        }

        // GET: Households/Details/5
        [Authorize(Roles = "Head of Household")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // GET: Households/Create
        [Authorize(Roles = "Guest")]
        public ActionResult Create()
        {
            if (User.IsInRole("Head of Household"))
            {
                return View("Error");
            }

            return View();
        }

        // POST: Households/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Name,Greeting")] Household household)
        {
            if (ModelState.IsValid)
            {
                if (!User.IsInRole("Admin"))
                {
                    rHelp.RemoveUserFromRole(User.Identity.GetUserId(), rHelp.ListUserRoles(User.Identity.GetUserId()).FirstOrDefault());
                }
                rHelp.AddUserToRole(User.Identity.GetUserId(), "Head of Household");
                household.Created = DateTime.Now;
                db.Households.Add(household);
                db.Users.Find(User.Identity.GetUserId()).HouseholdId = household.Id;
                db.SaveChanges();

                await ControllerContext.HttpContext.RefreshAuthentication(db.Users.Find(User.Identity.GetUserId()));
                return RedirectToAction("Index");
            }

            return View(household);
        }

        // GET: Households/Edit/5
        [Authorize(Roles = "Head of Household")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Greeting,Created")] Household household)
        {
            if (ModelState.IsValid)
            {
                db.Entry(household).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(household);
        }

        // GET: Households/Delete/5
        [Authorize(Roles = "Head of Household")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Household household = db.Households.Find(id);
            if (household == null)
            {
                return HttpNotFound();
            }
            return View(household);
        }

        // POST: Households/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Household household = db.Households.Find(id);
            db.Households.Remove(household);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Head of Household, Member")]
        public ActionResult RunAway()
        {
            var myRole = rHelp.ListUserRoles(User.Identity.GetUserId()).FirstOrDefault();
            var user = db.Users.Find(User.Identity.GetUserId());

            switch (myRole)
            {
                case "Head of Household":
                    var members = db.Users.Where(u => u.HouseholdId == user.HouseholdId).Count();
                    if (members > 1)
                    {
                        TempData["Message"] = $"";
                        return RedirectToAction("AppointSuccessor");
                    }

                    return View();

                case "Member":
                default:
                    return View();
            }

        }

        public async Task<ActionResult> Leave()
        {
            var myRole = rHelp.ListUserRoles(User.Identity.GetUserId()).FirstOrDefault();
            var user = db.Users.Find(User.Identity.GetUserId());

            switch (myRole)
            {
                case "Head of Household":
                    user.HouseholdId = null;
                    rHelp.RemoveUserFromRole(User.Identity.GetUserId(), "Head of Household");
                    rHelp.AddUserToRole(User.Identity.GetUserId(), "Guest");
                    db.SaveChanges();

                    await ControllerContext.HttpContext.RefreshAuthentication(db.Users.Find(User.Identity.GetUserId()));
                    return RedirectToAction("Dashboard", "Home");

                case "Member":
                default:
                    user.HouseholdId = null;
                    rHelp.RemoveUserFromRole(User.Identity.GetUserId(), "Member");
                    rHelp.AddUserToRole(User.Identity.GetUserId(), "Guest");
                    db.SaveChanges();

                    await ControllerContext.HttpContext.RefreshAuthentication(db.Users.Find(User.Identity.GetUserId()));
                    return RedirectToAction("Dashboard", "Home");
            }
        }

        [Authorize(Roles = "Head of Household")]
        public ActionResult AppointSuccessor()
        {
            var user = User.Identity.GetUserId();
            var householdId = db.Users.Find(user).HouseholdId ?? 0;

            if (householdId == 0)
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var members = db.Users.Where(u => u.HouseholdId == householdId && u.Id != user);
            ViewBag.Successor = new SelectList(members, "Id", "DisplayName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AppointSuccessor(string successor)
        {
            if (string.IsNullOrEmpty(successor))
            {
                return RedirectToAction("Dashboard", "Home");
            }

            var me = db.Users.Find(User.Identity.GetUserId());
            me.HouseholdId = null;
            rHelp.RemoveUserFromRole(me.Id, "Head of Household");
            rHelp.AddUserToRole(me.Id, "Guest");
            rHelp.RemoveUserFromRole(successor, "Member");
            rHelp.AddUserToRole(successor, "Head of Household");
            db.SaveChanges();

            nHelp.SendNewRoleNotification(successor, "Head of Household");

            await ControllerContext.HttpContext.RefreshAuthentication(me);
            return RedirectToAction("Dashboard", "Home");
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
