using CalendarApp.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;

namespace CalendarApp.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        

        [Authorize]
        public ActionResult Index()
        {
            if (Session["login"] == null)
            {
                return Redirect("~/user/login");
            }


            return View();
        }

        [Authorize]
        public JsonResult GetEvents()
        {
            using (CalendarAppDatabaseEntities1 context = new CalendarAppDatabaseEntities1())
            {
                var events = context.Events.ToList();
                return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }
        
        [Authorize]
        [HttpPost]
        public JsonResult SaveEvent(User user, Event e)
        {
            var status = false;
            using (CalendarAppDatabaseEntities1 context = new CalendarAppDatabaseEntities1())
            {
                if (e.EventID > 0)
                {
                    //Update the event
                    var v = context.Events.Where(a => a.EventID == e.EventID).FirstOrDefault();
                    if (v != null)
                    {
                        v.Subject = e.Subject;
                        v.Start = e.Start;
                        v.End = e.End;
                        v.Description = e.Description;
                        v.IsFullDay = e.IsFullDay;
                        v.ThemeColor = e.ThemeColor;
                    }
                }
                else
                {
                    context.Events.Add(e);
                }
                context.SaveChanges();
                status = true;
                using (DBManager db = new DBManager())
                {
                    
                    db.SaveChanges();
                    SendReminder(user.Email, e.Subject, e.Start);
                }
                }
            return new JsonResult { Data = new { status } };
        }

        [NonAction]
        public void SendReminder(string email, string eventinfo, DateTime eventStart)
        {
            var emily = HttpContext.User.Identity.Name; 
            
            var fromEmail = new MailAddress("test401.project401@gmail.com", "Project 401");
            var toEmail = new MailAddress(emily);
            var fromEmailPassword = "Testproject401";
            string subject = "Your account is successfully created!";

            string body = "<br/><br/>Hey, You just added an event to your calander." +
                "<br/><br/> Event Description: <br><br>" +eventinfo +" starting at "+eventStart;
                   

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword),

            };

            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })

                smtp.Send(message);

        }

        [Authorize]
        [HttpPost]
        public JsonResult DeleteEvent(int eventID)
        {
            var status = false;
            using (CalendarAppDatabaseEntities1 dc = new CalendarAppDatabaseEntities1())
            {
                var v = dc.Events.Where(a => a.EventID == eventID).FirstOrDefault();
                if (v != null)
                {
                    dc.Events.Remove(v);
                    dc.SaveChanges();
                    status = true;
                }
            }
            return new JsonResult { Data = new { status } };
        }
    }
}