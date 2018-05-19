using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    public class loginController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string logout)
        {
            if (logout == "yes")
            {
                FormsAuthentication.SignOut();
                Response.Cookies.Add(new HttpCookie("email")
                {
                    Expires = DateTime.Now.AddMinutes(-1)
                });
                Response.Cookies.Add(new HttpCookie("status")
                {
                    Expires = DateTime.Now.AddMinutes(-1)
                });
                Response.Cookies.Add(new HttpCookie("id") 
                { 
                    Expires = DateTime.Now.AddMinutes(-1) 
                });
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Index(string email,string password)
        {
            var users = db.users.Where(u => u.contact.email == email);
            if (users.Count() == 1)
            {
                int time = 30;
                string hash = users.First().passwordhash;
                if (VerifyHashedPassword(hash, password))
                {
                    var ticket = new FormsAuthenticationTicket(
                        2,
                        email,
                        DateTime.Now,
                        DateTime.Now.AddMinutes(time),
                        true,
                        users.First().status,
                        FormsAuthentication.FormsCookiePath
                    );
                    string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                    var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                    authCookie.Expires = DateTime.Now.AddMinutes(time);
                    var emailCookie = new HttpCookie("email", email);
                    emailCookie.Expires = DateTime.Now.AddMinutes(time);
                    var statusCookie = new HttpCookie("status", users.First().status);
                    statusCookie.Expires = DateTime.Now.AddMinutes(time);
                    var idCookie = new HttpCookie("id", users.First().userID.ToString());
                    idCookie.Expires = DateTime.Now.AddMinutes(time);
                    Response.Cookies.Add(authCookie);
                    Response.Cookies.Add(emailCookie);
                    Response.Cookies.Add(statusCookie);
                    Response.Cookies.Add(idCookie);
                    return RedirectToAction("../"+ FormsAuthentication.GetRedirectUrl(email,false));
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public static bool VerifyHashedPassword(string hashedPassword, string password)
        {
            byte[] buffer4;
            if (hashedPassword == null)
            {
                return false;
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            byte[] src = Convert.FromBase64String(hashedPassword);
            if ((src.Length != 0x31) || (src[0] != 0))
            {
                return false;
            }
            byte[] dst = new byte[0x10];
            Buffer.BlockCopy(src, 1, dst, 0, 0x10);
            byte[] buffer3 = new byte[0x20];
            Buffer.BlockCopy(src, 0x11, buffer3, 0, 0x20);
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, dst, 0x3e8))
            {
                buffer4 = bytes.GetBytes(0x20);
            }
            return buffer3.SequenceEqual(buffer4);
        }
    }
}