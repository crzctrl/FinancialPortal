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
    public class InvitationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Invitations
        public ActionResult Index()
        {
            var invitations = db.Invitations.Include(i => i.Household);
            return View(invitations.ToList());
        }

        // GET: Invitations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // GET: Invitations/Create
        public ActionResult Create()
        {
            //ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name");
            var houseId = db.Users.Find(User.Identity.GetUserId()).HouseholdId ?? 0;
            if (houseId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            //var invitation = new Invitation
            //{
            //    HouseholdId = houseId,
            //    TTL = 7
            //};
            var invitation = new Invitation();
            invitation.TTL += 7;
            invitation.HouseholdId = houseId;
            return View(invitation);
        }

        // POST: Invitations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,HouseholdId,TTL,RecipientEmail")] Invitation invitation)
        {            
            foreach (var email in db.Users.Select(u => u.Email).ToList())
            {
                if (email.ToLower() == invitation.RecipientEmail.ToLower())
                {
                    if (ModelState.IsValid)
                    {
                        invitation.Created = DateTime.Now;
                        invitation.Code = Guid.NewGuid();
                        invitation.IsValid = true;
                        db.Invitations.Add(invitation);
                        db.SaveChanges();

                        EmailService ems = new EmailService();
                        IdentityMessage msg = new IdentityMessage();
                        var householdName = db.Households.Find(invitation.HouseholdId).Name;
                        var callbackUrl = Url.Action("AcceptLoginInvitation", "Account", new { recipientEmail = invitation.RecipientEmail, code = invitation.Code }, protocol: Request.Url.Scheme);

                        msg.Body = $"<h3>Hey there, Hi there, Ho there!</h3>Click <a href=\"{callbackUrl}\">here</a> and login as a member of {householdName}.<br />This invitation will self-destruct in {invitation.TTL} days.";
                        msg.Destination = invitation.RecipientEmail;
                        msg.Subject = "Penny Pincher Invitation!";

                        await ems.SendMailAsync(msg);
                        return RedirectToAction("Dashboard", "Home");
                    }
                }
            }

            if (ModelState.IsValid)
            {
                invitation.Created = DateTime.Now;
                invitation.Code = Guid.NewGuid();
                invitation.IsValid = true;
                db.Invitations.Add(invitation);
                db.SaveChanges();

                EmailService ems = new EmailService();
                IdentityMessage msg = new IdentityMessage();
                var householdName = db.Households.Find(invitation.HouseholdId).Name;
                var callbackUrl = Url.Action("AcceptRegisterInvitation", "Account", new { recipientEmail = invitation.RecipientEmail, code = invitation.Code }, protocol: Request.Url.Scheme);

                msg.Body = $"<h3>Hey there, Hi there, Ho there!</h3>Click <a href=\"{callbackUrl}\">here</a> and register as a member of {householdName}.<br />This invitation will self-destruct in {invitation.TTL} days.";
                msg.Destination = invitation.RecipientEmail;
                msg.Subject = "Penny Pincher Invitation!";

                await ems.SendMailAsync(msg);
                return RedirectToAction("Dashboard", "Home");
            }

            //ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View("Error");
        }

        // GET: Invitations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View(invitation);
        }

        // POST: Invitations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,HouseholdId,IsValid,Created,TTL,RecipientEmail,Code")] Invitation invitation)
        {
            if (ModelState.IsValid)
            {
                db.Entry(invitation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.HouseholdId = new SelectList(db.Households, "Id", "Name", invitation.HouseholdId);
            return View(invitation);
        }

        // GET: Invitations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Invitation invitation = db.Invitations.Find(id);
            if (invitation == null)
            {
                return HttpNotFound();
            }
            return View(invitation);
        }

        // POST: Invitations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Invitation invitation = db.Invitations.Find(id);
            db.Invitations.Remove(invitation);
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
