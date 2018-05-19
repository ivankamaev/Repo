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
    public class organizationsController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var organizations = (IQueryable<organization>)db.organizations;
            ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            switch (sortOrder)
            {
                case "Name desc":
                    organizations = organizations.OrderByDescending(o => o.name);
                    break;
                default:
                    organizations = organizations.OrderBy(o => o.name);
                    break;
            }
            return View(organizations.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            organization organization = db.organizations.Find(id);
            if (organization == null)
            {
                return HttpNotFound();
            }
            return View(organization);
        }

        public ActionResult Create()
        {
            var organizations = db.organizations.OrderBy(o => o.organizationID);
            int i = 1;
            foreach (organization o in organizations)
            {
                if (i != o.organizationID)
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
        public ActionResult Create([Bind(Include = "organizationID,name,phone,email,note")] organization organization)
        {
            if (ModelState.IsValid)
            {
                db.organizations.Add(organization);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(organization);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            organization organization = db.organizations.Find(id);
            if (organization == null)
            {
                return HttpNotFound();
            }
            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "organizationID,name,phone,email,note")] organization organization)
        {
            if (ModelState.IsValid)
            {
                db.Entry(organization).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(organization);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            organization organization = db.organizations.Find(id);
            if (organization == null)
            {
                return HttpNotFound();
            }
            return View(organization);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            organization organization = db.organizations.Find(id);
            db.organizations.Remove(organization);
            IEnumerable<contact> contacts = db.contacts.Where(c => c.organizationID == id);
            foreach (contact p in contacts)
            {
                p.organizationID = null;
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
