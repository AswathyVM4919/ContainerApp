using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VerbatTest.Models;

namespace VerbatTest.Controllers
{
    [Authorize]
    public class ContainerController : Controller
    {
        private readonly ContainerManagementEntities DbContext = new ContainerManagementEntities();
        [HandleError(View = "Error")]
        public ActionResult Index()
        {
            try
            {
                List<Container> container = DbContext.Containers.ToList();
                return View(container);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HandleError(View = "Error")]
        public ActionResult Details(int id)
        {
            try
            {
                Container container = DbContext.Containers.Single(containers => containers.ContainerId == id);
                return View(container);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HandleError(View = "Error")]
        public ActionResult Create()
        {
            try
            {
                List<SelectListItem> statusList = new List<SelectListItem>();
                statusList.Add(new SelectListItem { Text = "Select status", Value = "", Selected = true });
                statusList.Add(new SelectListItem { Text = "Transit", Value = "Transit" });
                statusList.Add(new SelectListItem { Text = "At Dock", Value = "At Dock" });
                statusList.Add(new SelectListItem { Text = "Delivered", Value = "Delivered" });
                ViewBag.Status = statusList;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Container container, HttpPostedFileBase file)
        {
            try
            {
                List<SelectListItem> statusList = new List<SelectListItem>();
                statusList.Add(new SelectListItem { Text = "Select status", Value = "", Selected = true });
                statusList.Add(new SelectListItem { Text = "Transit", Value = "Transit" });
                statusList.Add(new SelectListItem { Text = "At Dock", Value = "At Dock" });
                statusList.Add(new SelectListItem { Text = "Delivered", Value = "Delivered" });
                ViewBag.Status = statusList;

                if (file != null)
                {
                    int maxSize = 5 * 1024 * 1024;
                    if (file.ContentLength > maxSize)
                    {
                        ModelState.AddModelError("file", "File size must not exceed 5 MB.");
                       ViewBag.Message = "File size must not exceed 5 MB.";
                        return View();
                    }

                    string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                    if (fileExtension != ".pdf")
                    {
                        ModelState.AddModelError("file", "Invalid file type. Only .pfd are allowed.");
                        ViewBag.Message = "Invalid file type. Only .pfd are allowed.";
                        return View();

                    }
                }
                if (ModelState.IsValid)
                {
                    if (DbContext.Containers.Any(x => x.ContainerNumber == container.ContainerNumber))
                    {
                        ViewBag.Message = "Container Number is already existed";
                        return View();
                    }
                    else
                    {
                        if (file != null && file.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(file.FileName);
                            string path = Path.Combine(Server.MapPath("~/FileStore"), fileName);
                            file.SaveAs(path);
                            container.FilePath = fileName;
                        }
                        DbContext.Containers.Add(container);
                        DbContext.SaveChanges();
                        return RedirectToAction("Index", "Container");
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
            }
            return View(container);
        }
        [HandleError(View = "Error")]
        public ActionResult Delete(int id)
        {
            try
            {
                if (id > 0)
                {
                    var contId = DbContext.Containers.Where(x => x.ContainerId == id).FirstOrDefault();
                    if (contId != null)
                    {
                        DbContext.Entry(contId).State = EntityState.Deleted;
                        DbContext.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return RedirectToAction("Index");
        }
        [HandleError(View = "Error")]
        public ActionResult Download(string FilePath)
        {
            try
            {
                if (string.IsNullOrEmpty(FilePath))
                {
                    return View();
                }
                else
                {
                    //string Path = System.Configuration.ConfigurationManager.AppSettings["FileStore"] + FilePath;
                    string filepath = Path.Combine(Server.MapPath("~/FileStore"), FilePath);
                    if (!System.IO.File.Exists(filepath))
                    {
                        return HttpNotFound();
                    }
                    return File(filepath, "application/pdf");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}