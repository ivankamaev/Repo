using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication.Attributes;
using WebApplication.Models;

namespace WebApplication1.Controllers
{
    [UserAuthorize]
    public class project_equipmentController : Controller
    {
        //private u0416457_systemEntities db = new u0416457_systemEntities();
        private u0516067_coopersystemEntities db = new u0516067_coopersystemEntities();

        public ActionResult Index(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var project_equipment = db.project_equipment.Include(pe => pe.projects).Include(pe => pe.equipment).Where(pe => pe.projectID == id).ToList();
            if (project_equipment == null)
            {
                return HttpNotFound();
            }
            ViewBag.ID = id;
            projects pr = db.projects.Find(id);
            if (pr.executorID != null)
            {
                ViewBag.ExecutorName = pr.contacts1.name;
                ViewBag.ExecutorLastname = pr.contacts1.lastname;
            }
            if (pr.placeID != null)
            {
                ViewBag.Place = pr.places.name;
            }
            if (pr.start != null)
            {
                ViewBag.Start = @Convert.ToDateTime(pr.start).ToString("dd.MM.yyyy hh:mm");
            }
            if (Request.Cookies.Get("id") != null)
            {
                if (((FormsIdentity)User.Identity).Ticket.UserData == "admin" || pr.createrID == null || pr.createrID.ToString() == Request.Cookies.Get("id").Value)
                {
                    ViewBag.Hide = "no";
                }
                else
                {
                    ViewBag.Hide = "yes";
                }
            }
            else
            {
                ViewBag.Hide = "yes";
            }

            return View(project_equipment);
        }

        public ActionResult Create(int? id)
        {
            if (Request.Cookies.Get("id") != null)
            {
                if (db.projects.Find(id).createrID != null)
                {
                    if (db.projects.Find(id).createrID.ToString() != Request.Cookies.Get("id").Value && ((FormsIdentity)User.Identity).Ticket.UserData != "admin")
                    {
                        return RedirectToAction("Index", "login");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "login");
            }

            var project_equipment = db.project_equipment.OrderBy(pe => pe.project_equipmentID);
            int i = 1;
            foreach (project_equipment pe in project_equipment)
            {
                if (i != pe.project_equipmentID)
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
            ViewBag.peID = i;
            ViewBag.ID = id;
            ViewBag.createrID = 1;
            projects pr = db.projects.Find(id);
            if (pr.executorID != null)
            {
                ViewBag.ExecutorName = pr.contacts1.name;
                ViewBag.ExecutorLastname = pr.contacts1.lastname;
            }
            if (pr.placeID != null)
            {
                ViewBag.Place = pr.places.name;
            }
            var equipment = db.equipment.Where(e => e.equipmentID != null).ToList();
            IEnumerable<SelectListItem> selectList1 = from e in equipment
                                                     where e.tech_models.tech_brands != null && e.tech_models != null
                                                     orderby e.tech_models.tech_brands.name, e.tech_models.name 
                                                     select new SelectListItem
                                                     {
                                                         Value = e.equipmentID.ToString(),
                                                         Text = e.tech_models.tech_brands.name + " " + e.tech_models.name
                                                     };
            IEnumerable<SelectListItem> selectList2 = from e in equipment
                                                      where e.tech_models.tech_brands == null
                                                      orderby e.tech_models.name
                                                      select new SelectListItem
                                                      {
                                                          Value = e.equipmentID.ToString(),
                                                          Text = e.tech_models.name
                                                      };
            IEnumerable<SelectListItem> selectList3 = from e in equipment
                                                      where e.tech_models == null
                                                      orderby e.tech_models.tech_brands.name
                                                      select new SelectListItem
                                                      {
                                                          Value = e.equipmentID.ToString(),
                                                          Text = e.tech_models.tech_brands.name
                                                      };

            IEnumerable<SelectListItem> selectList = selectList1.Concat(selectList2).Concat(selectList3);

            ViewBag.equipmentID = new SelectList(selectList, "Value", "Text");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "project_equipmentID,projectID,equipmentID,count,note")] project_equipment project_equipment)
        {
            if (ModelState.IsValid)
            {
                int i = Convert.ToInt32(project_equipment.projectID);
                db.project_equipment.Add(project_equipment);
                db.SaveChanges();
                return RedirectToAction("Index", new { id = i });
            }
            return View(project_equipment);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            project_equipment project_equipment = db.project_equipment.Find(id);
            if (project_equipment == null)
            {
                return HttpNotFound();
            }
            projects pr = db.projects.Find(project_equipment.projectID);
            ViewBag.ID = pr.projectID;
            if (Request.Cookies.Get("id") != null)
            {
                if (pr.createrID != null)
                {
                    if (pr.createrID.ToString() != Request.Cookies.Get("id").Value && ((FormsIdentity)User.Identity).Ticket.UserData != "admin")
                    {
                        return RedirectToAction("Index", "login");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "login");
            }
            if (pr.executorID != null)
            {
                ViewBag.ExecutorName = pr.contacts.name;
                ViewBag.ExecutorLastname = pr.contacts.lastname;
            }
            if (pr.placeID != null)
            {
                ViewBag.Place = pr.places.name;
            }
            return View(project_equipment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "project_equipmentID,projectID,equipmentID,count,note")] project_equipment project_equipment)
        {
            if (ModelState.IsValid)
            {
                int i = Convert.ToInt32(project_equipment.projectID);
                db.Entry(project_equipment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", new { id = i });
            }
            return View(project_equipment);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            project_equipment project_equipment = db.project_equipment.Find(id);
            if (project_equipment == null)
            {
                return HttpNotFound();
            }
            projects pr = db.projects.Find(project_equipment.projectID);
            ViewBag.ID = pr.projectID;
            if (Request.Cookies.Get("id") != null)
            {
                if (pr.createrID != null)
                {
                    if (pr.createrID.ToString() != Request.Cookies.Get("id").Value && ((FormsIdentity)User.Identity).Ticket.UserData != "admin")
                    {
                        return RedirectToAction("Index", "login");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "login");
            }
            if (pr.executorID != null)
            {
                ViewBag.ExecutorName = pr.contacts.name;
                ViewBag.ExecutorLastname = pr.contacts.lastname;
            }
            if (pr.placeID != null)
            {
                ViewBag.Place = pr.places.name;
            }
            return View(project_equipment);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            project_equipment project_equipment = db.project_equipment.Find(id);
            int i = Convert.ToInt32(project_equipment.projectID);
            db.project_equipment.Remove(project_equipment);
            db.SaveChanges();
            return RedirectToAction("Index", new { id = i });
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
