using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication.Attributes;
using WebApplication.Models;

namespace WebApplication.Controllers
{
    [UserAuthorize]
    public class contactsController : Controller
    {
       // private u0416457_systemEntities db = new u0416457_systemEntities();
        private u0516067_coopersystemEntities db = new u0516067_coopersystemEntities();

        public ActionResult Index(string sortOrder)
        {
            var contacts = db.contacts.Include(c => c.organizations);
            ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            ViewBag.OrganizationSort = sortOrder == "Organization" ? "Organization desc" : "Organization";
            switch (sortOrder)
            {
                case "Name desc":
                    contacts = contacts.OrderByDescending(c => c.lastname);
                    break;
                case "Organization":
                    contacts = contacts.OrderBy(c => c.organizations.name);
                    break;
                case "Organization desc":
                    contacts = contacts.OrderByDescending(c => c.organizations.name);
                    break;
                default:
                    contacts = contacts.OrderBy(c => c.lastname);
                    break;
            }
            return View(contacts.ToList());
        }


        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            contacts contact = db.contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        public ActionResult Create()
        {
            ViewBag.organizationID = new SelectList(db.organizations, "organizationID", "name");

            var contacts = db.contacts.Include(c => c.organizations).OrderBy(c => c.contactID);
            int i = 1;
            foreach (contacts c in contacts)
            {
                if (i != c.contactID)
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
        public ActionResult Create([Bind(Include = "contactID,name,lastname,organizationID,position,phone,email,note")] contacts contact)
        {
            if (ModelState.IsValid)
            {
                db.contacts.Add(contact);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.organizationID = new SelectList(db.organizations, "organizationID", "name", contact.organizationID);
            return View(contact);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            contacts contact = db.contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            ViewBag.organizationID = new SelectList(db.organizations, "organizationID", "name", contact.organizationID);
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "contactID,name,lastname,organizationID,position,phone,email,note")] contacts contact)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.organizationID = new SelectList(db.organizations, "organizationID", "name", contact.organizationID);
            return View(contact);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            contacts contact = db.contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            contacts contact = db.contacts.Find(id);
            db.contacts.Remove(contact);

            IEnumerable<projects> projects1 = db.projects.Where(p => p.executorID == id);
            foreach (projects p in projects1)
            {
                p.executorID = null;
            }
            IEnumerable<projects> projects2 = db.projects.Where(p => p.clientID == id);
            foreach (projects p in projects2)
            {
                p.clientID = null;
            }
            IEnumerable<projects> projects3 = db.projects.Where(p => p.managerID == id);
            foreach (projects p in projects3)
            {
                p.managerID = null;
            }
            IEnumerable<projects> projects4 = db.projects.Where(p => p.showmanID == id);
            foreach (projects p in projects4)
            {
                p.showmanID = null;
            }
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
