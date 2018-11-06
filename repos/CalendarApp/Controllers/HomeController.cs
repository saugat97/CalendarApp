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
    }
}