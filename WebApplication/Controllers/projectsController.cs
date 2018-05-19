using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;
using WebApplication.Attributes;
using WebApplication.Controllers;
using System.Web.Security;


namespace WebApplication1.Controllers
{
    [UserAuthorize]
    public class projectsController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index(string sortOrder)
        {
            var projects = db.projects.Include(c => c.contact).Include(c => c.contact1).Include(c => c.contact2).Include(c => c.contact3).Include(p => p.place).Include(u => u.user);
            ViewBag.DateSort = String.IsNullOrEmpty(sortOrder) ? "Date desc" : "";
            ViewBag.PlaceSort = sortOrder == "Place" ? "Place desc" : "Place";
            switch (sortOrder)
            {
                case "Date desc":
                    projects = projects.OrderByDescending(p => p.start);
                    break;
                case "Place":
                    projects = projects.OrderBy(p => p.place.name);
                    break;
                case "Place desc":
                    projects = projects.OrderByDescending(p => p.place.name);
                    break;
                default:
                    projects = projects.OrderBy(p => p.start);
                    break;
            }
            if (Request.Cookies.Get("id") != null)
            {
                ViewBag.UID = Request.Cookies.Get("id").Value;
            }
            else
            {
                ViewBag.UID = null;
            }
            if (Request.Cookies.Get("status").Value == "admin")
            {
                ViewBag.status = "admin";
            }
            return View(projects.ToList());
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            project project = db.projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            var pels = db.project_equipment.Include(pe => pe.project).Include(pe => pe.equipment).Where(pe => pe.projectID == id).ToList();
            List<string> name = new List<string>();
            List<string> brand = new List<string>();
            List<string> model = new List<string>();
            List<int> count = new List<int>();
            foreach (var item in pels)
            {
                if (item.equipment != null)
                {
                    name.Add(item.equipment.name);
                    if (item.equipment.tech_models != null)
                    {
                        model.Add(item.equipment.tech_models.name);
                        if (item.equipment.tech_models.tech_brands != null)
                        {
                            brand.Add(item.equipment.tech_models.tech_brands.name);
                        }
                        else
                        {
                            brand.Add("");
                        }
                    }
                    else
                    {
                        model.Add("");
                        brand.Add("");
                    }
                }
                else
                {
                    name.Add("");
                    model.Add("");
                    brand.Add("");
                }
                count.Add(Convert.ToInt32(item.count));
            }
            ViewBag.name = name;
            ViewBag.brand = brand;
            ViewBag.model = model;
            ViewBag.count = count;
            if (Request.Cookies.Get("id") != null)
            {
                ViewBag.UID = Request.Cookies.Get("id").Value;
            }
            else
            {
                ViewBag.UID = null;
            }
            if (Request.Cookies.Get("status").Value == "admin")
            {
                ViewBag.status = "admin";
            }
            return View(project);
        }

        public ActionResult Create()
        {
            var contactls = db.contacts.Where(c => c.contactID != null).ToList();
            IEnumerable<SelectListItem> selectList = from c in contactls
                                                     orderby c.lastname
                                                     select new SelectListItem
                                                     {
                                                         Value = c.contactID.ToString(),
                                                         Text = c.lastname + " " + c.name
                                                     };

            List<SelectListItem> stlist = new List<SelectListItem>();
            stlist.Add(new SelectListItem() { Value = "KM", Text = "Купер Может" });
            stlist.Add(new SelectListItem() { Value = "BD", Text = "BackgroundDJ" });
            stlist.Add(new SelectListItem() { Value = "KM/BD", Text = "Купер Может / BackgroundDJ" });
            ViewBag.worktype = new SelectList(stlist, "Value", "Text");

            ViewBag.status = new SelectList(stlist, "Value", "Text");
            ViewBag.executorID = new SelectList(selectList, "Value", "Text");
            ViewBag.clientID = new SelectList(selectList, "Value", "Text");
            ViewBag.managerID = new SelectList(selectList, "Value", "Text");
            ViewBag.showmanID = new SelectList(selectList, "Value", "Text");
            ViewBag.placeID = new SelectList(db.places, "placeID", "name");

            var projects = db.projects.Include(c => c.contact).Include(c => c.contact1).Include(c => c.contact2).Include(c => c.contact3).Include(p => p.place).Include(u => u.user).OrderBy(p => p.projectID);
            int i = 1;
            foreach (project pr in projects)
            {
                if (i != pr.projectID)
                {
                    break;
                }
                else
                {
                    i++;
                }
            }
            ViewBag.ID = i;
            if (Request.Cookies.Get("id") != null)
            {
                ViewBag.createrID = Request.Cookies.Get("id").Value;
            }
            else
            {
                ViewBag.createrID = null;
            }
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "projectID,createrID,arrival,installation,rehearsal,start,finish,deinstallation,departure,placeID,worktype,executorID,type,showmanID,managerID,clientID,content,note,receipts_cash,receipts_noncash,expenditure_cash,expenditure_noncash,profit_cash,profit_noncash,profit_total")] project project)
        {
            if (ModelState.IsValid)
            {
                db.projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.executorID = new SelectList(db.contacts, "contactID", "name", project.executorID);
            ViewBag.clientID = new SelectList(db.contacts, "contactID", "name", project.clientID);
            ViewBag.managerID = new SelectList(db.contacts, "contactID", "name", project.managerID);
            ViewBag.showmanID = new SelectList(db.contacts, "contactID", "name", project.showmanID);
            ViewBag.placeID = new SelectList(db.places, "placeID", "name", project.placeID);
            return View(project);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            project project = db.projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            if (Request.Cookies.Get("id") != null)
            {
                if (project.createrID != null)
                {
                    if (project.createrID.ToString() != Request.Cookies.Get("id").Value && ((FormsIdentity)User.Identity).Ticket.UserData != "admin")
                    {
                        return RedirectToAction("Index", "login");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "login");
            }

            var contactls = db.contacts.Where(c => c.contactID != null).ToList();
            IEnumerable<SelectListItem> selectList = from c in contactls
                                                     orderby c.lastname
                                                     select new SelectListItem
                                                     {
                                                         Value = c.contactID.ToString(),
                                                         Text = c.lastname + " " + c.name
                                                     };

            List<SelectListItem> stlist = new List<SelectListItem>();
            stlist.Add(new SelectListItem() { Value = "KM", Text = "Купер Может" });
            stlist.Add(new SelectListItem() { Value = "BD", Text = "BackgroundDJ" });
            stlist.Add(new SelectListItem() { Value = "KM/BD", Text = "Купер Может / BackgroundDJ" });
            ViewBag.worktype = new SelectList(stlist, "Value", "Text", project.worktype);

            ViewBag.executorID = new SelectList(selectList, "Value", "Text", project.executorID);
            ViewBag.clientID = new SelectList(selectList, "Value", "Text", project.clientID);
            ViewBag.managerID = new SelectList(selectList, "Value", "Text", project.managerID);
            ViewBag.showmanID = new SelectList(selectList, "Value", "Text", project.showmanID);
            ViewBag.placeID = new SelectList(db.places, "placeID", "name", project.placeID);
            if (project.arrival != null)
            {
                ViewBag.Arrival = Convert.ToDateTime(project.arrival).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            if (project.installation != null)
            {
                ViewBag.Installation = Convert.ToDateTime(project.installation).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            if (project.rehearsal != null)
            {
                ViewBag.Rehearsal = Convert.ToDateTime(project.rehearsal).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            if (project.start != null)
            {
                ViewBag.Start = Convert.ToDateTime(project.start).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            if (project.finish != null)
            {
                ViewBag.Finish = Convert.ToDateTime(project.finish).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            if (project.deinstallation != null)
            {
                ViewBag.Deinstallation = Convert.ToDateTime(project.deinstallation).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            if (project.departure != null)
            {
                ViewBag.Departure = Convert.ToDateTime(project.departure).ToString("yyyy-MM-ddTHH:mm:ss");
            }
            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "projectID,createrID,arrival,installation,rehearsal,start,finish,deinstallation,departure,placeID,worktype,executorID,type,showmanID,managerID,clientID,content,note,receipts_cash,receipts_noncash,expenditure_cash,expenditure_noncash,profit_cash,profit_noncash,profit_total")] project project)
        {
            if (ModelState.IsValid)
            {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.executorID = new SelectList(db.contacts, "personID", "name", project.executorID);
            ViewBag.clientID = new SelectList(db.contacts, "personID", "name", project.clientID);
            ViewBag.managerID = new SelectList(db.contacts, "personID", "name", project.managerID);
            ViewBag.showmanID = new SelectList(db.contacts, "personID", "name", project.showmanID);
            ViewBag.placeID = new SelectList(db.places, "placeID", "name", project.placeID);
            return View(project);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            project project = db.projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            if (Request.Cookies.Get("id") != null)
            {
                if (project.createrID != null)
                {
                    if (project.createrID.ToString() != Request.Cookies.Get("id").Value && ((FormsIdentity)User.Identity).Ticket.UserData != "admin")
                    {
                        return RedirectToAction("Index", "login");
                    }
                }
            }
            else
            {
                return RedirectToAction("Index", "login");
            }

            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            project project = db.projects.Find(id);
            db.projects.Remove(project);
            IEnumerable<project_equipment> project_equipment = db.project_equipment.Where(p => p.projectID == id);
            foreach (project_equipment pe in project_equipment)
            {
                db.project_equipment.Remove(pe);
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
