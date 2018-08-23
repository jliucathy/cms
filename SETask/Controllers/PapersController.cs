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
using System.IO;
using Microsoft.AspNet.Identity;
using System.Threading.Tasks;
using System.Net.Mail;
    
namespace SETask.Controllers
{
    //Paper Controller
    public class PapersController : Controller
    {
        //database, used to retrive information from database
        private SETask1 db = new SETask1();
        //ApplicationUserManager, user to manage the registered User.
        private ApplicationUserManager _userManager;
        //private ApplicationDbContext context;

          // initialise UserManager, which is used to manage the registed User
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

        // GET: Papers
        //return the list of Papers in a conference.
        //@param int, the conferenceId
        public ActionResult Index(int? ConferenceId)
        {
            ViewBag.CurrentUserId = User.Identity.GetUserId();
            if (ConferenceId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conference conference = db.Conferences.Find(ConferenceId);
            if (conference == null)
            {
                return HttpNotFound();
            }
            var papers = db.Papers.Include(p => p.AspNetUser).Include(p => p.Conference).Where(c=>c.ConferenceId == ConferenceId);
            return View(papers.ToList());
        }

        // GET: Papers/Details/5
        //return the detail of a specific paper
        //@param, int the paper Id
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Paper paper = db.Papers.Find(id);
            if (paper == null)
            {
                return HttpNotFound();
            }
            ViewBag.ArticleId = id.Value;

            var comments = db.Reviews.Where(r => r.PaperId.Equals(id.Value)).ToList();
            ViewBag.Comments = comments;

            var ratings = db.Reviews.Where(r => r.PaperId.Equals(id.Value)).ToList();
            if (paper.Reviews.Count() > 0 && paper.Reviews.Count() <= 4)
            {
                var ratingSum = ratings.Sum(r => r.Rating);
                ViewBag.RatingSum = ratingSum;
                var ratingCount = paper.Reviews.Count();
                ViewBag.RatingCount = ratingCount;
            }
            else
            {
                ViewBag.RatingSum = 0;
                ViewBag.RatingCount = 0;
            }
            return View(paper);
        }

        //Used to inform the author of the article, only Admin and Chair is allowed to call this method
        //@param String JournalistId, int paperId
        [Authorize(Roles ="Admin,Chair")]
        public ActionResult Inform(string id,int? paperId)
        {
                if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InformModel informModel = new InformModel();
            informModel.AuthorId = id;
            ViewBag.PaperId = paperId;
            return View(informModel);
        }

        //Used to inform the author of the article, only Admin and Chair is allowed to call this method
        //@param Email model
        //@return the sent page, if the email is sent successfully.
        [Authorize(Roles = "Admin,Chair")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Inform(InformModel model)
        {
            AspNetUser author = db.AspNetUsers.Find(model.AuthorId);
            if (ModelState.IsValid)
            {
                var body = "<p>Email From: {0} ({1})Message:</p><p>{2}</p>";
                var message = new MailMessage();
                message.To.Add(new MailAddress(author.Email));
                message.From = new MailAddress("jliu0030@student.monash.edu");
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
                    ViewBag.SUCCESS = true;
                }
            }
            return View(model);
        }

        //Used to read the content of the article, and present it in the View
        //@param theFileName
        //@return the the content Page of the
        [Authorize]
        public ActionResult ListFile(string FileName)
        {
            string filePath = Server.MapPath("~/Uploads/" + FileName);
            FileInfo file = new FileInfo(filePath);

            string code = ReadFile(filePath);
            ViewBag.Message = code;
            return View();
        }

        //Used to read the content of  a file
        //@param the filepath of the file
        //@return a String contains all the information of the file.
        private string ReadFile(string filepath)
        {
            string fileOutput = "";
            try
            {
                StreamReader FileReader = new StreamReader(filepath);
                //The returned value is -1 if no more characters are 
                //currently available. 
                while (FileReader.Peek() > -1)
                {
                    //ReadLine() Reads a line of characters from the 
                    //current stream and returns the data as a string. 
                    fileOutput += FileReader.ReadLine().Replace("<", "&lt;").
                    Replace(" ", "&nbsp;&nbsp;&nbsp;&nbsp;")
                                         + "<br />";
                }
                FileReader.Close();
            }
            catch (FileNotFoundException e)
            {
                fileOutput = e.Message;
            }
            return fileOutput;
        }

        // GET: Papers/Create
        //turn to the create Paper Page
        //@param int the conferenceId
        //@return  create Paper Page
        [Authorize(Roles ="Author")]
        public ActionResult Create(int? ConferenceId)
        {
            var currentUserId = User.Identity.GetUserId();
            if (ConferenceId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Conference conference = db.Conferences.Find(ConferenceId);
            if(conference == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorId = User.Identity.GetUserId();
            ViewBag.ConferenceId = ConferenceId;
            return View();
        }

        // POST: Papers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        //create a file and save it in the database, only author is allowed to call this method.
        //@param Paper
        //@return if the article is successfully created, then will return the  conference index page.
        [HttpPost]
        [Authorize(Roles = "Author")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PaperId,Title,keyWords,Abstract,PaperFile,ConferenceId,AuthorId,SubmitDate")] Paper paper,HttpPostedFileBase postedFile)
        {
            if (postedFile != null)
            {
                string path = Server.MapPath("~/Uploads/");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (System.IO.Path.GetExtension(postedFile.FileName) == ".txt")
                {
                    postedFile.SaveAs(path + postedFile.FileName);
                    paper.PaperFile = postedFile.FileName;
                }
                else
                {
                    ViewBag.Message = "Please upload a txt file";
                    return View(paper);
                }
            }
            paper.Published = false;
            paper.AuthorId = User.Identity.GetUserId();
            Conference conference = db.Conferences.Find(paper.ConferenceId);            
            int maxSubmittedPpaer = conference.MaximumNoOfPaperSubmitted;
            DateTime currentTime = new DateTime();
            currentTime = DateTime.Now;
            int compareResult = DateTime.Compare(currentTime, conference.PaperDeadLine);
            if (conference.Papers.Count() < maxSubmittedPpaer && compareResult<=0)
            {
                if (ModelState.IsValid)
                {
                    db.Papers.Add(paper);
                    db.SaveChanges();                    
                }
                return RedirectToAction("Index","Conferences");
            }
            else
            {
                ViewBag.error = "You cannot submitted paper now.";
                return View();
            }


            //return View(paper);
        }

        // GET: Papers/Edit/5
        // get the paper from the database, and turn to the edit page, only the chair of the conference is allowed to call this method.
        // @param int paperId, int onferenceId 
        // @return
        [Authorize(Roles = "Chair,Admin")]
        public ActionResult Edit(int? id,int? conferenceId)
        {
            if (id == null||conferenceId == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Paper paper = db.Papers.Find(id);
            Conference conference = db.Conferences.Find(conferenceId);
            if (paper == null||conference == null)
            {
                return HttpNotFound();
            }
            ViewBag.error = " ";
            ViewBag.AuthorId = new SelectList(db.AspNetUsers, "Id", "Email", paper.AuthorId);
            ViewBag.ConferenceId = conferenceId;
            return View(paper);
        }

        // POST: Papers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.

        //edit the paper, and only the chair and admin is allowed to edit the paper
        //@param the paper
        //@return if edited successfully, return the index page of conference
        [HttpPost]
        [Authorize(Roles = "Chair,Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaperId,Title,keyWords,Abstract,PaperFile,ConferenceId,AuthorId,SubmitDate,Published")] Paper paper)
        {

            var currentUserId = User.Identity.GetUserId();
            int currentConferenceId = paper.ConferenceId;
            Conference conference = db.Conferences.Find(currentConferenceId);
            int maxPublishedPpaer = conference.MaximumNoOfPaperPublished;
            int NoOfPublished =db.Papers.Where(p => p.ConferenceId == currentConferenceId).Where(p => p.Published == true).Count();
            ViewBag.AuthorId = new SelectList(db.AspNetUsers, "Id", "Email", paper.AuthorId);
            ViewBag.ConferenceId = paper.ConferenceId;
                //new SelectList(db.Conferences, "ConferenceId", "Conference_Name", paper.ConferenceId);
            ViewBag.ErrorMessage = "you have no right to modify the page";
            
            if (UserManager.FindById(currentUserId).Roles.FirstOrDefault().RoleId == "1" || db.Conferences.Find(paper.ConferenceId).ChairId == currentUserId)
            {

                if (ModelState.IsValid && NoOfPublished<= maxPublishedPpaer)
                {
                    db.Entry(paper).State = EntityState.Modified;
                    db.SaveChanges();
                    //return RedirectToAction("Index", "Papers",new { ConferenceId = currentConferenceId});
                    return RedirectToAction("Index", "Conferences");
                }
                else
                {
                    ViewBag.error = "Only" + maxPublishedPpaer.ToString() + "are allowed to be published";
                    return View();
                }
            }
            ViewBag.error = " you are not authorized to modify the page";
            return View(paper);
        }

        //find the paper from the paper database, only admin is allowed to call this method.
        //@param the paper Id
        //@return return to the delete page
        // GET: Papers/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Paper paper = db.Papers.Find(id);
            if (paper == null)
            {
                return HttpNotFound();
            }
            return View(paper);
        }

        // POST: Papers/Delete/5

        //confirm the delete of the paper
        //@param int paperId
        //@return if the paper is successfully deleted, return to the index Page of conference 
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Paper paper = db.Papers.Find(id);
            db.Papers.Remove(paper);
            db.SaveChanges();
            return RedirectToAction("Index","Conferences");
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
