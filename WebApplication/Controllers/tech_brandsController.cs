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
    public class tech_brandsController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var tech_brands = (IQueryable<tech_brands>)db.tech_brands;
            ViewBag.BrandSort = String.IsNullOrEmpty(sortOrder) ? "Brand desc" : "";
            switch (sortOrder)
            {
                case "Brand desc":
                    tech_brands = tech_brands.OrderByDescending(tb => tb.name);
                    break;
                default:
                    tech_brands = tech_brands.OrderBy(tb => tb.name);
                    break;
            }
            return View(tech_brands.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tech_brands tech_brand = db.tech_brands.Find(id);
            if (tech_brand == null)
            {
                return HttpNotFound();
            }
            return View(tech_brand);
        }

        public ActionResult Create()
        {
            var tech_brands = db.tech_brands.OrderBy(tb => tb.brandID);
            int i = 1;
            foreach (tech_brands tb in tech_brands)
            {
                if (i != tb.brandID)
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
        public ActionResult Create([Bind(Include = "brandID,name,description")] tech_brands tech_brand)
        {
            if (ModelState.IsValid)
            {
                db.tech_brands.Add(tech_brand);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tech_brand);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tech_brands tech_brand = db.tech_brands.Find(id);
            if (tech_brand == null)
            {
                return HttpNotFound();
            }
            return View(tech_brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "brandID,name,description")] tech_brands tech_brand)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tech_brand).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tech_brand);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tech_brands tech_brand = db.tech_brands.Find(id);
            if (tech_brand == null)
            {
                return HttpNotFound();
            }
            return View(tech_brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tech_brands tech_brand = db.tech_brands.Find(id);
            db.tech_brands.Remove(tech_brand);
            IEnumerable<tech_models> tech_models = db.tech_models.Where(tm => tm.brandID == id);
            foreach (tech_models tm in tech_models)
            {
                tm.brandID = null;
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