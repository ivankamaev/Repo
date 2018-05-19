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

namespace WebApplication1.Controllers
{
    [UserAuthorize]
    public class tech_modelsController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var tech_models = db.tech_models.Include(tm => tm.tech_brands);
            ViewBag.BrandSort = String.IsNullOrEmpty(sortOrder) ? "Brand desc" : "";
            ViewBag.NameSort = sortOrder == "Name" ? "Name desc" : "Name";
            ViewBag.TypeSort = sortOrder == "Type" ? "Type desc" : "Type";
            switch (sortOrder)
            {
                case "Brand desc":
                    tech_models = tech_models.OrderByDescending(tm => tm.tech_brands.name);
                    break;
                case "Name":
                    tech_models = tech_models.OrderBy(tm => tm.name);
                    break;
                case "Name desc":
                    tech_models = tech_models.OrderByDescending(tm => tm.name);
                    break;
                case "Type":
                    tech_models = tech_models.OrderBy(tm => tm.type);
                    break;
                case "Type desc":
                    tech_models = tech_models.OrderByDescending(tm => tm.type);
                    break;
                default:
                    tech_models = tech_models.OrderBy(tm => tm.tech_brands.name);
                    break;
            }
            return View(tech_models.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tech_models tech_model = db.tech_models.Find(id);
            if (tech_model == null)
            {
                return HttpNotFound();
            }
            return View(tech_model);
        }

        public ActionResult Create()
        {
            var tech_brands = db.tech_brands.Where(tb => tb.brandID != null).ToList();
            IEnumerable<SelectListItem> selectList = from tb in tech_brands
                                                     select new SelectListItem
                                                     {
                                                         Value = tb.brandID.ToString(),
                                                         Text = tb.name
                                                     };
            ViewBag.brandID = new SelectList(selectList, "Value", "Text");

            var tech_model = db.tech_models.Include(tm => tm.tech_brands).OrderBy(tm => tm.modelID);
            int i = 1;
            foreach (tech_models tm in tech_model)
            {
                if (i != tm.modelID)
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
        public ActionResult Create([Bind(Include = "modelID,name,brandID,characteristics,description,type")] tech_models tech_model)
        {
            if (ModelState.IsValid)
            {
                db.tech_models.Add(tech_model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.brandID = new SelectList(db.tech_brands, "brandID", "name", tech_model.brandID);
            return View(tech_model);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tech_models tech_model = db.tech_models.Find(id);
            if (tech_model == null)
            {
                return HttpNotFound();
            }
            var tech_brands = db.tech_brands.Where(tb => tb.brandID != null).ToList();
            IEnumerable<SelectListItem> selectList = from tb in tech_brands
                                                     select new SelectListItem
                                                     {
                                                         Value = tb.brandID.ToString(),
                                                         Text = tb.name
                                                     };
            ViewBag.brandID = new SelectList(selectList, "Value", "Text", tech_model.brandID);
            return View(tech_model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "modelID,name,brandID,characteristics,description,type")] tech_models tech_model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tech_model).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.brandID = new SelectList(db.tech_brands, "brandID", "name", tech_model.brandID);
            return View(tech_model);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            tech_models tech_model = db.tech_models.Find(id);
            if (tech_model == null)
            {
                return HttpNotFound();
            }
            return View(tech_model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            tech_models tech_model = db.tech_models.Find(id);
            db.tech_models.Remove(tech_model);
            IEnumerable<equipment> equipment = db.equipments.Where(e => e.modelID == id);
            foreach (equipment e in equipment)
            {
                e.modelID = null;
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
