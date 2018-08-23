using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SETask.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace SETask.Controllers
{
    public class ConferencesController : Controller
    {
        //database
        private SETask1 db = new SETask1();

        //userManager which is used to manage the users in the database
        private ApplicationUserManager _userManager;
        //private ApplicationDbContext context;

         //initialize the UserManager
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // the index of the conference 
        // @return return the index page of the conference
        // GET: Conferences
        public ActionResult Index()
        {
            var conferences = db.Conferences.Include(c => c.AspNetUser);
            return View(conferences.ToList());
        }

        // return the detail page of the conference
        // @param int ConferenceId
        // @return the detail page of the relevant Conference
        // GET: Conferences/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conference conference = db.Conferences.Find(id);
            if (conference == null)
            {
                return HttpNotFound();
            }
            return View(conference);
        }

        //turn to the create page of the conference, only chair and admin are allowed to call this method
        //@return turn to the Create Page 
        [Authorize(Roles = "Chair,Admin")]
        // GET: Conferences/Create
        public ActionResult Create()
        {
            ViewBag.ChairId = User.Identity.GetUserId();
            return View();
        }

        // POST: Conferences/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        // create a new conference, and save the conference in the database, only admin and chairs are allowed to call this method
        // @param Conference conference
        // @return The index page of the conferences
        [HttpPost]
        [Authorize(Roles = "Chair,Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ConferenceId,Conference_Name,PaperDeadLine,Location,StartingDate,MaximumNoOfPaperSubmitted,MaximumNoOfPaperPublished,ChairId")] Conference conference)
        {
            var currentUserId = User.Identity.GetUserId();
            conference.ChairId = currentUserId;
            if (ModelState.IsValid)
            {
                db.Conferences.Add(conference);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ChairId = User.Identity.GetUserId();
            return View(conference);
        }

        // turn to the edit page of the conference, only admin and chair are allowed to call this method 
        // @param int id the conferenceId 
        // @return return to the edit page of the conference
        // GET: Conferences/Edit/5
        [Authorize(Roles ="Admin,Chair")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conference conference = db.Conferences.Find(id);
            if (conference == null)
            {
                return HttpNotFound();
            }
            ViewBag.ErrorMessage = "";
            ViewBag.ChairId = new SelectList(db.AspNetUsers.Where(u=>u.AspNetRoles.FirstOrDefault().Id=="2"), "Id", "Email", conference.ChairId);
            return View(conference);
        }

        // POST: Conferences/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        // edit the Conference information, only admin and chair are allowed to call this method
        // @param Conference conference
        // @return return the index page of the conference
        [HttpPost]
        [Authorize(Roles = "Admin, Chair")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ConferenceId,Conference_Name,PaperDeadLine,Location,StartingDate,MaximumNoOfPaperSubmitted,MaximumNoOfPaperPublished,ChairId")] Conference conference)
        {
            var currentUserId = User.Identity.GetUserId();
            ViewBag.ErrorMessage = "you have no right to modify the page";
            ViewBag.ChairId = new SelectList(db.AspNetUsers.Where(u => u.AspNetRoles.FirstOrDefault().Id == "2"), "Id", "Email", conference.ChairId);
            if (currentUserId.Equals(conference.ChairId)||UserManager.FindById(currentUserId).Roles.FirstOrDefault().RoleId=="1")
            {
                if (ModelState.IsValid)
                {
                    db.Entry(conference).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return View();
            }

            return View(conference);
        }

        // turn to the delete page of the conference, only admin is allowed to call this method
        // @param int id
        // @return the delete page
        // GET: Conferences/Delete/5
        [Authorize(Roles ="Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conference conference = db.Conferences.Find(id);
            if (conference == null)
            {
                return HttpNotFound();
            }
            return View(conference);
        }

        // POST: Conferences/Delete/5

        //delete the Conference, only admin is allowed to delete the Conference
        //@param id
        //@return the index page of the conference
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Conference conference = db.Conferences.Find(id);
            db.Conferences.Remove(conference);
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
