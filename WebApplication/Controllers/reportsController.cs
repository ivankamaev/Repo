using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using System;
using System.Data;
using System.Data.Entity;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models;
using WebApplication.Attributes;


namespace WebApplication.Controllers
{
    [UserAuthorize]
    public class reportsController : Controller
    {
        private u0416457_systemEntities db = new u0416457_systemEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LineOptions()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LineOptions(DateTime begin, DateTime end)
        {
            TempData["begin"] = begin;
            TempData["end"] = end;

            return RedirectToAction("LineChart");
        }

        public ActionResult LineChart()
        {
            DateTime dtbegin = Convert.ToDateTime(TempData["begin"]);
            DateTime dtend = Convert.ToDateTime(TempData["end"]);

            Highcharts lineChart = new Highcharts("linechart");

            lineChart.InitChart(new Chart() { Type = DotNet.Highcharts.Enums.ChartTypes.Line });

            lineChart.SetTitle(new Title() { Text = "Отчет по доходам" });

            lineChart.SetXAxis(new XAxis()
            {
                Type = DotNet.Highcharts.Enums.AxisTypes.Datetime,
                DateTimeLabelFormats = new DateTimeLabel()
                {
                    Month = "%e. %b",
                    Year = "%b"
                },
                Title = new XAxisTitle() { Text = "Дата" }
            });

            lineChart.SetYAxis(new YAxis()
            {
                Title = new YAxisTitle() { Text = "Прибыль (руб)" },
                Min = 0
            });

            lineChart.SetPlotOptions(new PlotOptions()
            {
                Line = new PlotOptionsLine()
                {
                    Marker = new PlotOptionsLineMarker() { Enabled = true } 
                }
            });

            lineChart.SetSeries(new Series[]
            {
                new Series()
                {
                    Name = "Прибыль (итого)",
                    Data = new Data(ProfitStat(dtbegin, dtend))
                }
            });

            return View(lineChart);
        }

        public ActionResult PieOptions()
        {
            List<SelectListItem> stlist = new List<SelectListItem>();
            stlist.Add(new SelectListItem() { Value = "city", Text = "Города" });
            stlist.Add(new SelectListItem() { Value = "type", Text = "Тип проекта" });
            stlist.Add(new SelectListItem() { Value = "worktype", Text = "Функция на проекте" });
            stlist.Add(new SelectListItem() { Value = "client", Text = "Клиенты" });
            stlist.Add(new SelectListItem() { Value = "manager", Text = "Организаторы" });
            stlist.Add(new SelectListItem() { Value = "showman", Text = "Ведущие" });
            ViewBag.category = new SelectList(stlist, "Value", "Text");
            return View();
        }

        [HttpPost]
        public ActionResult PieOptions(DateTime begin, DateTime end, string category)
        {
            TempData["begin"] = begin;
            TempData["end"] = end;
            TempData["category"] = category;

            return RedirectToAction("PieChart");
        }

        public ActionResult PieChart()
        {
            DateTime dtbegin = Convert.ToDateTime(TempData["begin"]);
            DateTime dtend = Convert.ToDateTime(TempData["end"]);
            string cat = TempData["category"].ToString();
            ViewBag.Begin = dtbegin.ToString("dd.MM.yyyy hh:mm");
            ViewBag.End = dtend.ToString("dd.MM.yyyy hh:mm");

            Highcharts pieChart = new Highcharts("piechart");

            pieChart.InitChart(new Chart() { Type = DotNet.Highcharts.Enums.ChartTypes.Pie });

            pieChart.SetTitle(new Title() { Text = "Отчет по проектам" });

            pieChart.SetTooltip(new Tooltip() { PointFormat = "{series.name}: <b>{point.y}</b>" });

            PlotOptionsPie pop = new PlotOptionsPie()
            {
                AllowPointSelect = true,
                Cursor = DotNet.Highcharts.Enums.Cursors.Pointer,
                DataLabels = new PlotOptionsPieDataLabels() { Enabled = true, Format = "{point.name}: <b>{point.percentage:.1f}%</b>" }
            };
            pieChart.SetPlotOptions(new PlotOptions() { Pie = pop });

            pieChart.SetSeries(new Series[]
            {
                new Series()
                {
                    Name = "Проекты",
                    Data = new Data(CategoryStat(dtbegin, dtend, cat))
                }
            });

            return View(pieChart);
        }

        public ActionResult ColumnOptions()
        {
            var projects = db.projects;
            List<string> els = new List<string>();
            foreach (var p in projects)
            {
                els.Add(p.contact1.lastname + " " + p.contact1.name);
            }
            els = els.Distinct().ToList();
            List<SelectListItem> exlist = new List<SelectListItem>();
            foreach (var e in els)
            {
                exlist.Add(new SelectListItem() { Value = e, Text = e });
            }

            List<SelectListItem> plist = new List<SelectListItem>();
            plist.Add(new SelectListItem() { Value = "mon", Text = "Месяц" });
            plist.Add(new SelectListItem() { Value = "quar", Text = "Квартал" });
            plist.Add(new SelectListItem() { Value = "hyear", Text = "Полгода" });
            plist.Add(new SelectListItem() { Value = "year", Text = "Год" });

            ViewBag.period = new SelectList(plist, "Value", "Text");
            ViewBag.executors = new SelectList(exlist, "Value", "Text");
            return View();
        }

        [HttpPost]
        public ActionResult ColumnOptions(string period, string[] executors)
        {
            TempData["period"] = period;
            TempData["executors"] = executors.ToList();

            return RedirectToAction("ColumnChart");
        }

        public ActionResult ColumnChart()
        {
            string per = TempData["period"].ToString();
            List<string> ex = (List<string>)TempData["executors"];

            List<Series> ss = ExecutorStat(per,ex);

            string[] cat = new string[ss[0].Data.SeriesData.Length];
            for (int i = 0; i < cat.Length; i++)
            {
                cat[i] = ss[0].Data.SeriesData[i].Name;
            }



            Highcharts columnChart = new Highcharts("columnchart");

            columnChart.InitChart(new Chart() { Type = DotNet.Highcharts.Enums.ChartTypes.Column });

            columnChart.SetTitle(new Title() { Text = "Отчет по исполнителям" });

            columnChart.SetXAxis(new XAxis()
            {
                Title = new XAxisTitle() { Text = "Месяцы" },
                Categories = cat
            });

            columnChart.SetYAxis(new YAxis()
            {
                Title = new YAxisTitle() { Text = "Количество проектов" },
                ShowFirstLabel = true,
                ShowLastLabel = true,
                Min = 0
            });

            columnChart.SetLegend(new Legend() { Enabled = true });

            columnChart.SetSeries(ss.ToArray());

            return View(columnChart);
        }

        public Point[] CategoryStat(DateTime dtbegin, DateTime dtend, string cat)
        {
            var projects = db.projects.Where(p => p.start >= dtbegin).Where(p => p.start <= dtend);

            List<string> ls = new List<string>();
            List<Point> pls = new List<Point>();
            foreach (var pr in projects)
            {
                switch (cat)
                {
                    case "city":
                        ViewBag.Category = "Города";
                        if (pr.place.city != null)
                        {
                            ls.Add(pr.place.city);
                        }
                        break;
                    case "type":
                        ViewBag.Category = "Тип проекта";
                        if (pr.type != null)
                        {
                            ls.Add(pr.type);
                        } 
                        break;
                    case "worktype":
                        ViewBag.Category = "Функция на проекте";
                        if (pr.worktype != null)
                        {
                            if (pr.worktype == "KM")
                            {
                                ls.Add("Купер Может");
                            } else
                            {
                                if (pr.worktype == "BD")
                                {
                                    ls.Add("BackgroundDJ");
                                } else
                                {
                                    ls.Add("Купер Может / BackgroundDJ");
                                }
                            }
                        }
                        break;
                    case "client":
                        ViewBag.Category = "Клиенты";
                        if (pr.contact != null || pr.contact != null)
                        {
                            ls.Add(pr.contact.lastname + " " + pr.contact.name);
                        }
                        break;
                    case "manager":
                        ViewBag.Category = "Организаторы";
                        if (pr.contact2 != null || pr.contact2 != null)
                        {
                            ls.Add(pr.contact2.lastname + " " + pr.contact2.name);
                        }
                        break;
                    case "showman":
                        ViewBag.Category = "Ведущие";
                        if (pr.contact3 != null || pr.contact3 != null)
                        {
                            ls.Add(pr.contact3.lastname + " " + pr.contact3.name);
                        }
                        break;
                }
            }

            List<string> lsd = ls.Distinct().ToList();
            foreach (var l in lsd)
            {
                int i = 0;
                while (ls.Remove(l))
                {
                    i++;
                }
                pls.Add(new Point() { Name = l, Y = i });
            }
            Point[] arr = pls.ToArray();

            for (int i = 0; i < arr.Length - 1; i++)
            {
                for (int j = 0; j < arr.Length - i - 1; j++)
                {
                    if (arr[j].Y < arr[j+1].Y)
                    {
                        Point temp = arr[j];
                        arr[j] = arr[j + 1];
                        arr[j + 1] = temp;
                    }
                }
            }


            if (arr.Length <= 10)
            {
                return arr;
            }
            else
            {
                Point oth = new Point() { Name = "Другие" };
                int sum = 0;
                for (int i = arr.Length - 1; i > 9; i--)
                {
                    sum += (int)arr[i].Y;
                }
                oth.Y = sum;
                Point[] res = new Point[11];
                for (int i = 0; i < 10; i++)
                {
                    res[i] = arr[i];
                }
                res[10] = oth;
                return res;
            }
        }

        public List<Series> ExecutorStat(string per, List<string> ex)
        {
            List<int> monnumls = new List<int>();
            DateTime now = DateTime.Now;
            DateTime dtbegin = DateTime.Now;
            DateTime dtend = DateTime.Now;
            DateTime dttemp = DateTime.Now;
            switch (per)
            {
                case "mon":
                    monnumls.Add(DateTime.Now.Month);
                    dtbegin = new DateTime(now.Year, now.Month, 1);
                    break;
                case "quar":
                    for (int i = 0; i < 3; i++)
                    {
                        monnumls.Add(DateTime.Now.AddMonths(-i).Month);
                    }
                    dtbegin = new DateTime(now.Year, now.Month, 1).AddMonths(-2);
                    break;
                case "hyear":
                    for (int i = 0; i < 6; i++)
                    {
                        monnumls.Add(DateTime.Now.AddMonths(-i).Month);
                    }
                    dtbegin = new DateTime(now.Year, now.Month, 1).AddMonths(-5);
                    break;
                case "year":
                    for (int i = 0; i < 12; i++)
                    {
                        monnumls.Add(DateTime.Now.AddMonths(-i).Month);
                    }
                    dtbegin = new DateTime(now.Year, now.Month, 1).AddMonths(-11);
                    break;
            }
            monnumls.Reverse();
            string[] mons = { "Янв", "Фев", "Март", "Апр", "Май", "Июнь", "Июль", "Авг", "Сен", "Окт", "Нояб", "Дек" };
            List<string> monls = new List<string>();
            foreach (var m in monnumls)
            {
                monls.Add(mons[m-1]);
            }
            List<string> exls = new List<string>();
            if (ex.Count > 5)
            {
                for (int i = 0; i < 5; i++)
                {
                    exls.Add(ex[i]);
                }
            }
            else
            {
                exls = ex;
            }

            List<Series> ss = new List<Series>();
            foreach (var e in exls)
            {
                Series s = new Series();
                s.Name = e;
                List<Point> pp = new List<Point>();
                dttemp = dtbegin;
                for (int i = 0; i < monls.Count; i++)
                {
                    Point p = new Point();
                    p.Name = monls[i];
                    string ln = e.Substring(0, e.IndexOf(" "));
                    string n = e.Substring(e.IndexOf(" ") + 1, e.Length - e.IndexOf(" ") - 1);
                    dtend = dttemp.AddMonths(1).AddDays(-1);
                    p.Y = db.projects.Where(pr => pr.contact1.lastname == ln).Where(pr => pr.contact1.name == n).Where(pr => pr.start >= dttemp).Where(pr => pr.start <= dtend).Count();
                    dttemp = dttemp.AddMonths(1);
                    pp.Add(p);
                }
                s.Data = new Data(pp.ToArray());
                ss.Add(s);
            }

            return ss;
        }

        public Point[] ProfitStat(DateTime dtbegin, DateTime dtend)
        {
            var projects = db.projects.Where(p => p.start >= dtbegin).Where(p => p.start <= dtend).OrderBy(p => p.start).ToList();
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            List<Point> pls = new List<Point>();
            foreach (var pr in projects)
            {
                if (pr.start != null && pr.profit_total != null)
                {
                    DateTime dt = TimeZoneInfo.ConvertTimeToUtc(Convert.ToDateTime(pr.start));
                    TimeSpan ts = dt.Subtract(dt1970);
                    Point pt = new Point();
                    pt.X = Convert.ToInt64(ts.TotalMilliseconds);
                    pt.Y = pr.profit_total;
                    pls.Add(pt);
                }
            }
            return pls.ToArray();
        }

    }
}