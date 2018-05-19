using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;
using WebApplication.Attributes;

namespace WebApplication.Controllers
{
    [AdminAuthorize]
    public class usersController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var users = db.users.Include(u => u.contact);
            ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            switch (sortOrder)
            {
                case "Name desc":
                    users = users.OrderByDescending(u => u.contact.lastname);
                    break;
                default:
                    users = users.OrderBy(u => u.contact.lastname);
                    break;
            }
            return View(users.ToList());
        }

        public ActionResult Create()
        {
            var existusers = db.users.Where(u => u.userID != null).ToList();
            var contacts = db.contacts.Where(c => c.contactID != null).ToList();
            var cont = new List<contact>();
            foreach (contact c in contacts)
            {
                foreach (user u in existusers)
                {
                    if (c.contactID == u.contactID)
                    {
                        goto point;
                    }
                }
                cont.Add(c);
                point: continue;
            }
            IEnumerable<SelectListItem> selectList = from c in cont
                                                     select new SelectListItem
                                                     {
                                                         Value = c.contactID.ToString(),
                                                         Text = c.lastname + " " + c.name
                                                     };
            ViewBag.contactID = new SelectList(selectList, "Value", "Text");

            SelectListItem st1 = new SelectListItem();
            st1.Value = "admin";
            st1.Text = "Администратор";
            SelectListItem st2 = new SelectListItem();
            st2.Value = "user";
            st2.Text = "Пользователь";
            List<SelectListItem> stlist = new List<SelectListItem>();
            stlist.Add(st1);
            stlist.Add(st2);
            ViewBag.status = new SelectList(stlist, "Value", "Text");

            var users = db.users.OrderBy(u => u.userID);
            int i = 1;
            foreach (user u in users)
            {
                if (i != u.userID)
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
            ViewBag.ID = i;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "userID,contactID,status")] user user, string passwordhash)
        {
            if (ModelState.IsValid)
            {
                string hash = HashPassword(passwordhash);
                user.passwordhash = hash;
                db.users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.contactID = new SelectList(db.contacts, "contactID", "name", user.contactID);
            return View(user);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            var existusers = db.users.Where(u => u.userID != null).ToList();
            var contacts = db.contacts.Where(c => c.contactID != null).ToList();
            var cont = new List<contact>();
            cont.Add(user.contact);
            foreach (contact c in contacts)
            {
                foreach (user u in existusers)
                {
                    if (c.contactID == u.contactID)
                    {
                        goto point;
                    }
                }
                cont.Add(c);
            point: continue;
            }
            IEnumerable<SelectListItem> selectList = from c in cont
                                                     select new SelectListItem
                                                     {
                                                         Value = c.contactID.ToString(),
                                                         Text = c.lastname + " " + c.name
                                                     };
            ViewBag.contactID = new SelectList(selectList, "Value", "Text", user.contactID);

            SelectListItem st1 = new SelectListItem();
            st1.Value = "admin";
            st1.Text = "Администратор";
            SelectListItem st2 = new SelectListItem();
            st2.Value = "user";
            st2.Text = "Пользователь";
            List<SelectListItem> stlist = new List<SelectListItem>();
            stlist.Add(st1);
            stlist.Add(st2);
            ViewBag.status = new SelectList(stlist, "Value", "Text", user.status);

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "userID,contactID,passwordhash,status")] user user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }
   
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            user user = db.users.Find(id);
            db.users.Remove(user);
            IEnumerable<project> projects = db.projects.Where(p => p.createrID == id);
            foreach (project p in projects)
            {
                p.createrID = null;
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
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
