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
using System.Net.Mail;


namespace WebApplication1.Controllers
{
    [UserAuthorize]
    public class projectsController : Controller
    {
        private u0516067_coopersystemEntities db = new u0516067_coopersystemEntities();

        public ActionResult Index(string sortOrder)
        {
            var projects = db.projects.Include(c => c.contacts).Include(c => c.contacts1).Include(c => c.contacts2).Include(c => c.contacts3).Include(p => p.places).Include(u => u.users);
            ViewBag.DateSort = String.IsNullOrEmpty(sortOrder) ? "Date desc" : "";
            ViewBag.PlaceSort = sortOrder == "Place" ? "Place desc" : "Place";
            switch (sortOrder)
            {
                case "Date desc":
                    projects = projects.OrderByDescending(p => p.start);
                    break;
                case "Place":
                    projects = projects.OrderBy(p => p.places.name);
                    break;
                case "Place desc":
                    projects = projects.OrderByDescending(p => p.places.name);
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
            projects project = db.projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            var pels = db.project_equipment.Include(pe => pe.projects).Include(pe => pe.equipment).Where(pe => pe.projectID == id).ToList();
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

            var projects = db.projects.Include(c => c.contacts).Include(c => c.contacts1).Include(c => c.contacts2).Include(c => c.contacts3).Include(p => p.places).Include(u => u.users).OrderBy(p => p.projectID);
            int i = 1;
            foreach (projects pr in projects)
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
        public ActionResult Create([Bind(Include = "projectID,createrID,arrival,installation,rehearsal,start,finish,deinstallation,departure,placeID,worktype,executorID,type,showmanID,managerID,clientID,content,note,receipts_cash,receipts_noncash,expenditure_cash,expenditure_noncash,profit_cash,profit_noncash,profit_total")] projects project)
        {
            if (ModelState.IsValid)
            {
                db.projects.Add(project);
                db.SaveChanges();

                if (project.executorID != null && db.contacts.Find(project.executorID).email != null)
                {
                    String toEmail = db.contacts.Find(project.executorID).email;
                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                    client.Credentials = new NetworkCredential("ivankamaev94@gmail.com", "231564897Qwerty");
                    client.EnableSsl = true;
                    MailMessage mess = new MailMessage();
                    mess.From = new MailAddress("ivankamaev94@gmail.com");
                    mess.To.Add(new MailAddress(toEmail));
                    mess.Subject = "Новый проект";
                    String body = "Создан новый проект с вашим участием. <br><br> <table cellspacing=\"0\" cellpadding=\"10\"> <caption><b>Информация о проекте</b></caption>";
                    if (project.createrID != null && (db.contacts.Find(db.users.Find(project.createrID).contactID).lastname != null || db.contacts.Find(db.users.Find(project.createrID).contactID).name != null))
                    {
                        body += "<tr><td><b>Создатель</b></td><td>" + db.contacts.Find(db.users.Find(project.createrID).contactID).lastname + " " + db.contacts.Find(db.users.Find(project.createrID).contactID).name + "</td></tr>";
                    }
                    if (project.executorID != null && (db.contacts.Find(project.executorID).lastname != null || db.contacts.Find(project.executorID).name != null))
                    {
                        body += "<tr><td><b>Исполнитель</b></td><td>" + db.contacts.Find(project.executorID).lastname + " " + db.contacts.Find(project.executorID).name + "</td></tr>";
                    }
                    if (project.arrival != null)
                    {
                        body += "<tr><td><b>Заезд</b></td><td>" + Convert.ToDateTime(project.arrival).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
                    }
                    if (project.installation != null)
                    {
                        body += "<tr><td><b>Монтаж</b></td><td>" + Convert.ToDateTime(project.installation).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
                    }
                    if (project.rehearsal != null)
                    {
                        body += "<tr><td><b>Репетиции</b></td><td>" + Convert.ToDateTime(project.rehearsal).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
                    }
                    if (project.start != null)
                    {
                        body += "<tr><td><b>Начало</b></td><td>" + Convert.ToDateTime(project.start).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
                    }
                    if (project.finish != null)
                    {
                        body += "<tr><td><b>Конец</b></td><td>" + Convert.ToDateTime(project.finish).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
                    }
                    if (project.deinstallation != null)
                    {
                        body += "<tr><td><b>Демонтаж</b></td><td>" + Convert.ToDateTime(project.deinstallation).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
                    }
                    if (project.departure != null)
                    {
                        body += "<tr><td><b>Отъезд</b></td><td>" + Convert.ToDateTime(project.departure).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
                    }
                    if (project.placeID != null && db.places.Find(project.placeID).name != null)
                    {
                        body += "<tr><td><b>Место</b></td><td>" + db.places.Find(project.placeID).name + "</td></tr>";
                    }
                    if (project.worktype != null)
                    {
                        body += "<tr><td><b>Функция на проекте</b></td><td>" + project.worktype + "</td></tr>";
                    }
                    if (project.type != null)
                    {
                        body += "<tr><td><b>Тип проекта</b></td><td>" + project.type + "</td></tr>";
                    }
                    if (project.managerID != null && (db.contacts.Find(project.managerID).lastname != null || db.contacts.Find(project.managerID).name != null))
                    {
                        body += "<tr><td><b>Организатор</b></td><td>" + db.contacts.Find(project.managerID).lastname + " " + db.contacts.Find(project.managerID).name + "</td></tr>";
                    }
                    if (project.clientID != null && (db.contacts.Find(project.clientID).lastname != null || db.contacts.Find(project.clientID).name != null))
                    {
                        body += "<tr><td><b>Клиент</b></td><td>" + db.contacts.Find(project.clientID).lastname + " " + db.contacts.Find(project.clientID).name + "</td></tr>";
                    }
                    if (project.showmanID != null && (db.contacts.Find(project.showmanID).lastname != null || db.contacts.Find(project.showmanID).name != null))
                    {
                        body += "<tr><td><b>Ведущий</b></td><td>" + db.contacts.Find(project.showmanID).lastname + " " + db.contacts.Find(project.showmanID).name + "</td></tr>";
                    }
                    if (project.content != null)
                    {
                        body += "<tr><td><b>Контент</b></td><td>" + project.content + "</td></tr>";
                    }
                    if (project.note != null)
                    {
                        body += "<tr><td><b>Заметки</b></td><td>" + project.note + "</td></tr>";
                    }
                    body += "</table>";
                    mess.Body = body;
                    mess.IsBodyHtml = true;
                    client.Send(mess);
                }

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
            projects project = db.projects.Find(id);
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
        public ActionResult Edit([Bind(Include = "projectID,createrID,arrival,installation,rehearsal,start,finish,deinstallation,departure,placeID,worktype,executorID,type,showmanID,managerID,clientID,content,note,receipts_cash,receipts_noncash,expenditure_cash,expenditure_noncash,profit_cash,profit_noncash,profit_total")] projects project)
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
            projects project = db.projects.Find(id);
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
            projects project = db.projects.Find(id);
            String toEmail = null;
            if (project.executorID != null && db.contacts.Find(project.executorID).email != null)
            {
                toEmail = db.contacts.Find(project.executorID).email;
            }
            String body = "Проект с вашим участием удален. <br><br> <table cellspacing=\"0\" cellpadding=\"10\"> <caption><b>Информация о проекте</b></caption>";
            if (project.createrID != null && (db.contacts.Find(db.users.Find(project.createrID).contactID).lastname != null || db.contacts.Find(db.users.Find(project.createrID).contactID).name != null))
            {
                body += "<tr><td><b>Создатель</b></td><td>" + db.contacts.Find(db.users.Find(project.createrID).contactID).lastname + " " + db.contacts.Find(db.users.Find(project.createrID).contactID).name + "</td></tr>";
            }
            if (project.executorID != null && (db.contacts.Find(project.executorID).lastname != null || db.contacts.Find(project.executorID).name != null))
            {
                body += "<tr><td><b>Исполнитель</b></td><td>" + db.contacts.Find(project.executorID).lastname + " " + db.contacts.Find(project.executorID).name + "</td></tr>";
            }
            if (project.arrival != null)
            {
                body += "<tr><td><b>Заезд</b></td><td>" + Convert.ToDateTime(project.arrival).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
            }
            if (project.installation != null)
            {
                body += "<tr><td><b>Монтаж</b></td><td>" + Convert.ToDateTime(project.installation).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
            }
            if (project.rehearsal != null)
            {
                body += "<tr><td><b>Репетиции</b></td><td>" + Convert.ToDateTime(project.rehearsal).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
            }
            if (project.start != null)
            {
                body += "<tr><td><b>Начало</b></td><td>" + Convert.ToDateTime(project.start).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
            }
            if (project.finish != null)
            {
                body += "<tr><td><b>Конец</b></td><td>" + Convert.ToDateTime(project.finish).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
            }
            if (project.deinstallation != null)
            {
                body += "<tr><td><b>Демонтаж</b></td><td>" + Convert.ToDateTime(project.deinstallation).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
            }
            if (project.departure != null)
            {
                body += "<tr><td><b>Отъезд</b></td><td>" + Convert.ToDateTime(project.departure).ToString("dd.MM.yyyy HH:mm") + "</td></tr>";
            }
            if (project.placeID != null && db.places.Find(project.placeID).name != null)
            {
                body += "<tr><td><b>Место</b></td><td>" + db.places.Find(project.placeID).name + "</td></tr>";
            }
            if (project.worktype != null)
            {
                body += "<tr><td><b>Функция на проекте</b></td><td>" + project.worktype + "</td></tr>";
            }
            if (project.type != null)
            {
                body += "<tr><td><b>Тип проекта</b></td><td>" + project.type + "</td></tr>";
            }
            if (project.managerID != null && (db.contacts.Find(project.managerID).lastname != null || db.contacts.Find(project.managerID).name != null))
            {
                body += "<tr><td><b>Организатор</b></td><td>" + db.contacts.Find(project.managerID).lastname + " " + db.contacts.Find(project.managerID).name + "</td></tr>";
            }
            if (project.clientID != null && (db.contacts.Find(project.clientID).lastname != null || db.contacts.Find(project.clientID).name != null))
            {
                body += "<tr><td><b>Клиент</b></td><td>" + db.contacts.Find(project.clientID).lastname + " " + db.contacts.Find(project.clientID).name + "</td></tr>";
            }
            if (project.showmanID != null && (db.contacts.Find(project.showmanID).lastname != null || db.contacts.Find(project.showmanID).name != null))
            {
                body += "<tr><td><b>Ведущий</b></td><td>" + db.contacts.Find(project.showmanID).lastname + " " + db.contacts.Find(project.showmanID).name + "</td></tr>";
            }
            if (project.content != null)
            {
                body += "<tr><td><b>Контент</b></td><td>" + project.content + "</td></tr>";
            }
            if (project.note != null)
            {
                body += "<tr><td><b>Заметки</b></td><td>" + project.note + "</td></tr>";
            }
            body += "</table>";

            db.projects.Remove(project);
            IEnumerable<project_equipment> project_equipment = db.project_equipment.Where(p => p.projectID == id);
            foreach (project_equipment pe in project_equipment)
            {
                db.project_equipment.Remove(pe);
            }
            db.SaveChanges();

            if (toEmail != null)
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.Credentials = new NetworkCredential("ivankamaev94@gmail.com", "231564897Qwerty");
                client.EnableSsl = true;
                MailMessage mess = new MailMessage();
                mess.From = new MailAddress("ivankamaev94@gmail.com");
                mess.To.Add(new MailAddress(toEmail));
                mess.Subject = "Проект удален";
                mess.Body = body;
                mess.IsBodyHtml = true;
                client.Send(mess);
            }

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
