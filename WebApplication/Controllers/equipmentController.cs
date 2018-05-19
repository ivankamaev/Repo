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
    public class equipmentController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var equipments = db.equipments.Include(e => e.tech_models);
            ViewBag.NameSort = String.IsNullOrEmpty(sortOrder) ? "Name desc" : "";
            ViewBag.BrandSort = sortOrder == "Brand" ? "Brand desc" : "Brand";
            ViewBag.ModelSort = sortOrder == "Model" ? "Model desc" : "Model";
            ViewBag.TypeSort = sortOrder == "Type" ? "Type desc" : "Type";
            switch (sortOrder)
            {
                case "Name desc":
                    equipments = equipments.OrderByDescending(e => e.name);
                    break;
                case "Brand":
                    equipments = equipments.OrderBy(e => e.tech_models.tech_brands.name);
                    break;
                case "Brand desc":
                    equipments = equipments.OrderByDescending(e => e.tech_models.tech_brands.name);
                    break;
                case "Model":
                    equipments = equipments.OrderBy(e => e.tech_models.name);
                    break;
                case "Model desc":
                    equipments = equipments.OrderByDescending(e => e.tech_models.name);
                    break;
                case "Type":
                    equipments = equipments.OrderBy(e => e.tech_models.type);
                    break;
                case "Type desc":
                    equipments = equipments.OrderByDescending(e => e.tech_models.type);
                    break;
                default:
                    equipments = equipments.OrderBy(e => e.name);
                    break;
            }
            return View(equipments.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            equipment equipment = db.equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            return View(equipment);
        }

        public ActionResult Create()
        {
            var tech_models = db.tech_models.Where(tm => tm.modelID != null).ToList();
            IEnumerable<SelectListItem> selectList1 = from tm in tech_models
                                                      where tm.tech_brands != null
                                                      orderby tm.tech_brands.name, tm.name
                                                      select new SelectListItem
                                                      {
                                                          Value = tm.modelID.ToString(),
                                                          Text = tm.tech_brands.name + " " + tm.name
                                                      };

            IEnumerable<SelectListItem> selectList2 = from tm in tech_models
                                                      where tm.tech_brands == null
                                                      orderby tm.name
                                                      select new SelectListItem
                                                      {
                                                          Value = tm.modelID.ToString(),
                                                          Text = tm.name
                                                      };

            IEnumerable<SelectListItem> selectList = selectList1.Concat(selectList2);
            
            ViewBag.modelID = new SelectList(selectList, "Value", "Text");

            var equipment = db.equipments.Include(e => e.tech_models).OrderBy(e => e.equipmentID);
            int i = 1;
            foreach (equipment e in equipment)
            {
                if (i != e.equipmentID)
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
        public ActionResult Create([Bind(Include = "equipmentID,name,modelID,mark,count,note")] equipment equipment)
        {
            if (ModelState.IsValid)
            {
                db.equipments.Add(equipment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.modelID = new SelectList(db.tech_models, "modelID", "name", equipment.modelID);
            return View(equipment);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            equipment equipment = db.equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            var tech_models = db.tech_models.Where(tm => tm.modelID != null).ToList();
            IEnumerable<SelectListItem> selectList1 = from tm in tech_models
                                                      where tm.tech_brands != null
                                                      orderby tm.tech_brands.name, tm.name
                                                      select new SelectListItem
                                                      {
                                                          Value = tm.modelID.ToString(),
                                                          Text = tm.tech_brands.name + " " + tm.name
                                                      };
            IEnumerable<SelectListItem> selectList2 = from tm in tech_models
                                                      where tm.tech_brands == null
                                                      orderby tm.name
                                                      select new SelectListItem
                                                      {
                                                          Value = tm.modelID.ToString(),
                                                          Text = tm.name
                                                      };

            IEnumerable<SelectListItem> selectList = selectList1.Concat(selectList2);
            ViewBag.modelID = new SelectList(selectList, "Value", "Text", equipment.modelID);
            return View(equipment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "equipmentID,name,modelID,mark,count,note")] equipment equipment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(equipment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.brandmodelID = new SelectList(db.tech_models, "modelID", "name", equipment.modelID);
            return View(equipment);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            equipment equipment = db.equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            return View(equipment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            equipment equipment = db.equipments.Find(id);
            db.equipments.Remove(equipment);
            IEnumerable<project_equipment> project_equipment = db.project_equipment.Where(pe => pe.equipmentID == id);
            foreach (project_equipment pe in project_equipment)
            {
                pe.equipmentID = null;
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
