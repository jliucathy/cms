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
using System.Threading.Tasks;
using System.Net.Mail;
using Microsoft.AspNet.Identity;
namespace SETask.Controllers
{
    public class ReviewsController : Controller
    {
        //the database
        private SETask1 db = new SETask1();

        // GET: Reviews

        // the index of the Reviews, only Reviewer and admin is allowed to see the Reviewer
        [Authorize(Roles = "Reviewer, Admin")]
        public ActionResult Index()
        {

            var currentUserId = User.Identity.GetUserId();

            if (!db.AspNetUsers.Find(currentUserId).AspNetRoles.FirstOrDefault().Name.Equals("Admin"))
            {
                var reviews = db.Reviews.Include(r => r.AspNetUser).Include(r => r.Paper).Where(r => r.ReviewerId == currentUserId);
                return View(reviews.ToList());
            }
            else
            {
                var reviews = db.Reviews.Include(r => r.AspNetUser).Include(r => r.Paper);
                return View(reviews.ToList());
            }

        }

        // GET: Reviews/Details/5
        // the detail of the Reviews, only Reviewer and admin is allowed to see the Reviewer
        // @param int reviewId
        // @return the review detail page
        [Authorize(Roles = "Reviewer, Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // turn to the inform Page, which is not used in the program
        // @param String reviewerId
        // @return the inform page
        public ActionResult Inform(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InformModel informModel = new InformModel();
            informModel.AuthorId = id;
            return View(informModel);
        }

        //send email to the reviewer, which is not used in the program
        // @pram InformModel model
        // @return if the email is successfully sent, return the index of the page 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Inform(InformModel model)
        {
            AspNetUser reviewer = db.AspNetUsers.Find(model.AuthorId);
            if (ModelState.IsValid)
            {
                var body = "<p>Email From: {0} ({1})Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress(reviewer.Email));
                message.From = new MailAddress("jliu0030@student.monash.edu");//modify later
                message.Subject = model.Subject;
                message.Body = string.Format(body, message.From, message.From, model.Message);
                message.IsBodyHtml = true;

                HttpPostedFileBase postedFile = Request.Files["upload"];
                model.Upload = postedFile;
                if (model.Upload != null && model.Upload.ContentLength > 0)
                {
                    message.Attachments.Add(new Attachment(model.Upload.InputStream, System.IO.Path.GetFileName(model.Upload.FileName)));
                }

                using (var smtp = new SmtpClient())
                {
                    var credential = new NetworkCredential
                    {
                        UserName = "jliu0030@student.monash.edu",  // send email account 
                        Password = ""
                    };
                    smtp.Credentials = credential;
                    smtp.Host = "smtp.monash.edu.au";   //mail server
                                                        //smtp.Port = 587;
                                                        // smtp.EnableSsl = true;
                                                        // smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    await smtp.SendMailAsync(message);
                    ViewBag.SUCCESS = true; //???
                }
            }
            return View(model);
        }

        //Prepare the creation of a review, only chair and admin are allowed to call this method
        //@param int PaperId 
        //@return  the creation page for the paper


        // GET: Reviews/Create
        [Authorize(Roles = "Chair,Admin")]
        public ActionResult Create(int? paperId)
        {
            if (paperId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Paper paper = db.Papers.Find(paperId);
            if (paper == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReviewerId = new SelectList(db.AspNetUsers.Where(u => u.AspNetRoles.FirstOrDefault().Name.Equals("Reviewer")), "Id", "Email");
            ViewBag.PaperId = paperId;
            return View();
        }

        // POST: Reviews/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        // Create a review and save it in db, only admin and chair are allowed to call this method
        // @param Review review 
        // @return paper Detail page
        [HttpPost]
        [Authorize(Roles = "Chair,Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReviewId,Rating,Comment,PaperId,ReviewerId")] Review review)
        {
            int paperId = review.PaperId;
            List<Review> Reviews = db.Reviews.Where(r => r.PaperId == paperId).ToList();
            if (ModelState.IsValid && db.Papers.Find(review.PaperId).Reviews.Count() <= 4 && Reviews.Where(r => r.ReviewerId == review.ReviewerId).Count() == 0)
            {
                db.Reviews.Add(review);
                db.SaveChanges();
                var actionlink = "Details/" + review.PaperId;
                return RedirectToAction(actionlink, "Papers");
            }
            else
            {
                ViewBag.errorMessage = "Review is not allowed to be created";
            }
            ViewBag.ReviewerId = new SelectList(db.AspNetUsers.Where(u => u.AspNetRoles.FirstOrDefault().Name.Equals("Reviewer")), "Id", "Email");
            //ViewBag.PaperId = new SelectList(db.Papers, "PaperId", "Title", review.PaperId);
            return View(review);
        }

        // GET: Reviews/Edit/5
        // turn to the edit page of the review,only reviewers and admin are allowed to call this method
        // @param int reviewid
        // @return the edit page of the review

        [Authorize(Roles = "Reviewer,Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.ReviewerId = new SelectList(db.AspNetUsers, "Id", "Email", review.ReviewerId);
            ViewBag.PaperId = new SelectList(db.Papers, "PaperId", "Title", review.PaperId);
            return View(review);
        }

        // POST: Reviews/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        // edit the review, only admin and reviewers are allowed to call this method.
        // @param Review review
        // @return return the index page of the review
        [HttpPost]
        [Authorize(Roles = "Reviewer,Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ReviewId,Rating,Comment,PaperId,ReviewerId")] Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ReviewerId = new SelectList(db.AspNetUsers, "Id", "Email", review.ReviewerId);
            ViewBag.PaperId = new SelectList(db.Papers, "PaperId", "Title", review.PaperId);
            return View(review);
        }

        // prepare for the delete option, retrive relative information from the db, only admin is allowed to call this method.
        // @param int reviewId
        // @return return the delete page
        // GET: Reviews/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        // confirm the delete of a particular review,  and save in the db. Only admin is allowed to call this method.
        // @param int reviewId
        // @return the index page of the reviews
        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
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
