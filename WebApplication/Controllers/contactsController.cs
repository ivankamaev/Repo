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
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var contacts = db.contacts.Include(c => c.organization);
            ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            ViewBag.OrganizationSort = sortOrder == "Organization" ? "Organization desc" : "Organization";
            switch (sortOrder)
            {
                case "Name desc":
                    contacts = contacts.OrderByDescending(c => c.lastname);
                    break;
                case "Organization":
                    contacts = contacts.OrderBy(c => c.organization.name);
                    break;
                case "Organization desc":
                    contacts = contacts.OrderByDescending(c => c.organization.name);
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
            contact contact = db.contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        public ActionResult Create()
        {
            ViewBag.organizationID = new SelectList(db.organizations, "organizationID", "name");

            var contacts = db.contacts.Include(c => c.organization).OrderBy(c => c.contactID);
            int i = 1;
            foreach (contact c in contacts)
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
        public ActionResult Create([Bind(Include = "contactID,name,lastname,organizationID,position,phone,email,note")] contact contact)
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
            contact contact = db.contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            ViewBag.organizationID = new SelectList(db.organizations, "organizationID", "name", contact.organizationID);
            return View(contact);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "contactID,name,lastname,organizationID,position,phone,email,note")] contact contact)
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
            contact contact = db.contacts.Find(id);
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
            contact contact = db.contacts.Find(id);
            db.contacts.Remove(contact);

            IEnumerable<project> projects1 = db.projects.Where(p => p.executorID == id);
            foreach (project p in projects1)
            {
                p.executorID = null;
            }
            IEnumerable<project> projects2 = db.projects.Where(p => p.clientID == id);
            foreach (project p in projects2)
            {
                p.clientID = null;
            }
            IEnumerable<project> projects3 = db.projects.Where(p => p.managerID == id);
            foreach (project p in projects3)
            {
                p.managerID = null;
            }
            IEnumerable<project> projects4 = db.projects.Where(p => p.showmanID == id);
            foreach (project p in projects4)
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
