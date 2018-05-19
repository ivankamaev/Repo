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
    public class placesController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var places = (IQueryable<place>)db.places;
            ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            ViewBag.TypeSort = sortOrder == "Type" ? "Type desc" : "Type";
            ViewBag.CitySort = sortOrder == "City" ? "City desc" : "City";
            switch (sortOrder)
            {
                case "Name desc":
                    places = places.OrderByDescending(p => p.name);
                    break;
                case "Type":
                    places = places.OrderBy(p => p.type);
                    break;
                case "Type desc":
                    places = places.OrderByDescending(p => p.type);
                    break;
                case "City":
                    places = places.OrderBy(p => p.city);
                    break;
                case "City desc":
                    places = places.OrderByDescending(p => p.city);
                    break;
                default:
                    places = places.OrderBy(p => p.name);
                    break;
            }
            return View(places.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            place place = db.places.Find(id);
            if (place == null)
            {
                return HttpNotFound();
            }
            return View(place);
        }

        public ActionResult Create()
        {
            var places = db.places.OrderBy(p => p.placeID);
            int i = 1;
            foreach (place p in places)
            {
                if (i != p.placeID)
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
        public ActionResult Create([Bind(Include = "placeID,name,type,address,city,note,phone,email")] place place)
        {
            if (ModelState.IsValid)
            {
                db.places.Add(place);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(place);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            place place = db.places.Find(id);
            if (place == null)
            {
                return HttpNotFound();
            }
            return View(place);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "placeID,name,type,address,city,note,phone,email")] place place)
        {
            if (ModelState.IsValid)
            {
                db.Entry(place).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(place);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            place place = db.places.Find(id);
            if (place == null)
            {
                return HttpNotFound();
            }
            return View(place);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            place place = db.places.Find(id);
            db.places.Remove(place);
            IEnumerable<project> projects = db.projects.Where(p => p.placeID == id);
            foreach (project p in projects)
            {
                p.placeID = null;
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
