using CalendarApp.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CalendarApp.Controllers
{
    public class UserController : Controller
    {
        //Register
        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Exclude = "IsEmailVerified,ActivationCode")] User user)
        {
            bool Status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                //Check Email Existancy
                var emailExist = EmailAlreadyExist(user.Email);
                if (emailExist)
                {
                    ModelState.AddModelError("EmailExist", "Email alraedy exist");
                    return View(user);
                }

                //Register Activation Code
                user.ActivationCode = Guid.NewGuid();

                //Hash Password
                user.Password = Crypto.HashPassword(user.Password);
                user.ConfirmPassword = Crypto.HashPassword(user.ConfirmPassword);

                user.IsEmailVerified = false;

                using(DBManager context = new DBManager())
                {
                    context.Users.Add(user);
                    context.SaveChanges();

                    SendLinkToEmail(user.Email, user.ActivationCode.ToString());
                    message = " Please login to your email to Verify and Activate your account";
                    Status = true;
                }
            }


            else
            {
                message = "Invalid Request";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        //Verify Account
        [HttpGet]
        public ActionResult VerifyAccount(string id)
        {
            bool Status = false;
            using (DBManager context = new DBManager())
            {
                context.Configuration.ValidateOnSaveEnabled = false; //comfirm password is not in the database so this code avoids it

                var verifyEmail = context.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (verifyEmail != null)
                {
                    verifyEmail.IsEmailVerified = true;
                    context.SaveChanges();
                    Status = true;
                }
                else
                {
                    ViewBag.Message = "Invalid Request";
                }
            }
            ViewBag.Status = Status;
            return View();
        }

        //Get Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login login, string ReturnUrl = "")
        {
            string message = "";
            using (DBManager context = new DBManager())
            {
                var v = context.Users.Where(a => a.Email == login.Email).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(Crypto.HashPassword(login.Password), v.Password) == 0)
                    {
                        int timeout = login.RememberMe ? 3600 : 25;                             
                        var ticket = new FormsAuthenticationTicket(login.Email, login.RememberMe, timeout);
                        string encrypted = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);


                        Session["login"] = login.Email;
                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                           return Redirect(ReturnUrl);
                        }
                        else
                        {
                             return Redirect("~/Home/Index");
                        }
                    }
                    else
                    {
                        message = "Invalid credentials provided!";
                    }
                }
                else
                {
                    message = "Invalid credentials provided!";
                }
            }


            ViewBag.Message = message;
            return View();

        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }


        [NonAction]
        public bool EmailAlreadyExist(string email)
        {
            using (DBManager context = new DBManager())
            {
                var userEmail = context.Users.Where(a => a.Email == email).FirstOrDefault();
                return userEmail != null; // false : true;
            }
        }

        [NonAction]
        public void SendLinkToEmail(string email, string activationCode)
        {
            var verifyUrl = "/User/VerifyAccount/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);

            var fromEmail = new MailAddress("test401.project401@gmail.com", "Project 401");
            var toEmail = new MailAddress(email);
            var fromEmailPassword = "Testproject401";
            string subject = "Your account is successfully created!";

            string body = "<br/><br/>Your account is created successfully." +
                    "Please click on the link below to verify your account" +
                    "<br/><br/><a href='" + link + "'>" + link + "</a> ";

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
    }
}