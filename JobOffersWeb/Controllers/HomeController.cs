using JobOffersWeb.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using WebApplication3.Models;

namespace WebApplication3.Controllers
{
    public class HomeController : Controller  
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        public ActionResult Index()
        {

            return View(db.Categories.ToList());
        }

        public ActionResult Details( int  JobId)
        {
            var job = db.Jobs.Find(JobId);
            if (job ==null)
            {
                return HttpNotFound();
            }
            Session["JobId"] = JobId;
            return View(job);
        }

        [Authorize]
        public ActionResult Apply()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Apply(string Message)
        {
            var UserId = User.Identity.GetUserId();
            var JobId = (int)Session["JobId"];

            var apply = new ApplyForJob();
            var check = db.ApplyForJobs.Where(a => a.JobId == JobId && a.UserId == UserId).ToList();
            if (check.Count < 1)
            {
                apply.UserId = UserId;
                apply.JobId = JobId;
                apply.Message = Message;
                apply.ApplyDate = DateTime.Now;
                db.ApplyForJobs.Add(apply);
                db.SaveChanges();
                ViewBag.Result = "تمت الاضافة بنجاح ";
            }
            else
            {
                ViewBag.Result = "المعذرة،لقد سبق وتقدمت الى نفس الوظيفة ";

            }
            return View();

        }

        [Authorize]
        public ActionResult GetJobsByUser()
        {
            var UserId = User.Identity.GetUserId();
            var Jobs = db.ApplyForJobs.Where(a => a.UserId == UserId);
            return View(Jobs.ToList());
        }

        [Authorize]
        public ActionResult DetailsJobs(int id)
        {
            var job = db.ApplyForJobs.Find(id);
            if (job == null)
            {
                return HttpNotFound();

            }
            return View(job);
        }

        //Get Edit
        public ActionResult Edit(int id)
        {
            var edit = db.ApplyForJobs.Find(id);
            if (edit == null)
            {
                return HttpNotFound();
            }
            return View(edit);
        }

        // POST: /Edit/
        [HttpPost]

        public ActionResult Edit(ApplyForJob job)
        {
            if (ModelState.IsValid)
            {
                job.ApplyDate = DateTime.Now;
                db.Entry(job).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("GetJobsByUser");
            }
            return View(job);

        }

        // GET: Delete/5
        public ActionResult Delete(int id)
        {

            return View(db.Roles.Find(id));
        }

        // POST: Delete/5
        [HttpPost]

        public ActionResult Delete(ApplyForJob job)
        {
            var delete = db.ApplyForJobs.Find(job.Id);
            db.ApplyForJobs.Remove(delete);
            db.SaveChanges();
            return RedirectToAction("Index");

        }


        [Authorize]
        public ActionResult GetJobPublisher()
        {
            var userid = User.Identity.GetUserId();
            var Jobs = from app in db.ApplyForJobs
                       join job in db.Jobs
                       on app.JobId equals job.Id
                       where job.User.Id == userid
                       select app;

            var grouped = from j in Jobs
                          group j by j.job.JobTitle
                        into gr
                          select new JobsViewModel
                          {
                              JobTitle = gr.Key,
                              Items = gr
                          };

            return View(grouped.ToList());
        }

        public ActionResult Search()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Search(string searchName)
        {
            var result
                = db.Jobs.Where(
              a => a.JobContent.Contains(searchName)
            || a.JobTitle.Contains(searchName)
            || a.Category.CategoryName.Contains(searchName)
            || a.Category.CategoryDescription.Contains(searchName)).ToList();
            return View(result.ToList());
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public ActionResult Contact(ContactModel contact)
        {
            var mail = new MailMessage();
            var LoginInfo = new NetworkCredential( "ayatissa12@gmail.com", "554578793");
            mail.From = new MailAddress(contact.Email);
            mail.To.Add(new MailAddress("ayatissa12@gmail.com"));
            mail.Subject = contact.Subject;
            mail.IsBodyHtml = true;
            string body = "اسم المرسل " + contact.Name + "<br>"
                + "البريد المرسل :" + contact.Email + "<br>"
                + "عنوان الرسالة:" + contact.Subject + "<br>"
                + "نص الرسالة : <b>" + contact.Message + "</b>";




            mail.Body = body;
            var smtpClient = new SmtpClient("smtp.gmail.com", 587);
            smtpClient.EnableSsl = true;
            smtpClient.Credentials = LoginInfo;
            smtpClient.Send(mail);
            return RedirectToAction ("Index");
        }
    }
}