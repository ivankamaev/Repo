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
        //private u0416457_systemEntities db = new u0416457_systemEntities();
        private u0516067_coopersystemEntities db = new u0516067_coopersystemEntities();

        public ActionResult Index(string sortOrder)
        {
            var organizations = (IQueryable<organizations>)db.organizations;
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
            organizations organization = db.organizations.Find(id);
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
            foreach (organizations o in organizations)
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
        public ActionResult Create([Bind(Include = "organizationID,name,phone,email,note")] organizations organization)
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
            organizations organization = db.organizations.Find(id);
            if (organization == null)
            {
                return HttpNotFound();
            }
            return View(organization);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "organizationID,name,phone,email,note")] organizations organization)
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
            organizations organization = db.organizations.Find(id);
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
            organizations organization = db.organizations.Find(id);
            db.organizations.Remove(organization);
            IEnumerable<contacts> contacts = db.contacts.Where(c => c.organizationID == id);
            foreach (contacts p in contacts)
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
