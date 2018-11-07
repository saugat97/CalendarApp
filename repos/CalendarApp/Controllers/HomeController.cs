using CalendarApp.Models;
using System.Linq;
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
        public JsonResult SaveEvent(Event e)
        {
            var status = false;
            using (CalendarAppDatabaseEntities1 dc = new CalendarAppDatabaseEntities1())
            {
                if (e.EventID > 0)
                {
                    //Update the event
                    var v = dc.Events.Where(a => a.EventID == e.EventID).FirstOrDefault();
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
                    dc.Events.Add(e);
                }
                dc.SaveChanges();
                status = true;
            }
            return new JsonResult { Data = new { status } };
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