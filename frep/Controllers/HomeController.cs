using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DevExtreme.AspNet.Data;
using frep.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using frep.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace frep.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _appEnvironment;
 
        public HomeController(ILogger<HomeController> logger, 
            ApplicationDbContext context, 
            IWebHostEnvironment appEnvironment)
        {
            _logger = logger;
            _context = context;
            _appEnvironment = appEnvironment;
        }

        public IActionResult Index()
        {
            var documents = _context.Documents.Where(doc => !doc.IsSecured).ToList();
            ViewData["Title"] = "Нормативно-правовые акты ФСБ";
            ViewData["Secured"] = false;
            return View("Table", documents);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            var documents = _context.Documents.Where(doc => doc.IsSecured).ToList();
            ViewData["Title"] = "Документы ограниченного доступа";
            ViewData["Secured"] = true;
            return View("Table", documents);
        }
        
        public IActionResult GetDocument(int id)
        {
            return GetDocumentInternal(id, false);
        }
        
        [Authorize]
        public IActionResult GetSecuredDocument(int id)
        {
            return GetDocumentInternal(id, true);
        }

        [Authorize]
        public ActionResult AddDocument()
        {
            ViewData["Method"] = "AddDocument";
            return View("DocumentAddition");
        }
        
        [Authorize]
        public ActionResult AddSecureDocument()
        {
            ViewData["Method"] = "AddSecureDocument";
            return View("DocumentAddition");
        }
        
        [Authorize]
        [HttpPost]
        public ActionResult AddDocument(IFormFile selectedFile, string name, string category)
        {
            ViewData["Method"] = "AddDocument";
            return AddFile(selectedFile, name, category, false);
        }
        
        [Authorize]
        [HttpPost]
        public ActionResult AddSecureDocument(IFormFile selectedFile, string name, string category)
        {
            ViewData["Method"] = "AddSecureDocument";
            return AddFile(selectedFile, name, category, true);
        }

        private ActionResult AddFile(IFormFile selectedFile, string name, string category, bool isSecured)
        {
            if (!(selectedFile.FileName.Split('\\'))[^1].Contains("pdf"))
            {
                ViewData["Message"] = "Поддерживаются только файлы pdf";
                return View("DocumentAddition");
            }
            
            var date = DateTime.Now;
            var dateString = date.ToString()
                                 .Replace(":", "_")
                                 .Replace(" ", "_")
                                 .Replace(".", "_") + "_";

            var nameCleared = name
                .Replace(" ", "_")
                .Replace("\"", "_");
            string path = isSecured
                ? "\\files_secured\\" + dateString + nameCleared + ".pdf"
                : "\\files\\" + dateString + nameCleared + ".pdf";

            try
            {
                var newDocument = new Document { Name = name, Category = category, Path = path, DateAdd = date, IsSecured = isSecured };
                _context.Documents.Add(newDocument);
                _context.SaveChanges();

                using (var fileStream = new FileStream(_appEnvironment.ContentRootPath + path, FileMode.Create))
                {
                    selectedFile.CopyTo(fileStream);
                }

                ViewData["Message"] = "Файл успешно загружен";
                return View("DocumentAddition");
            }
            catch
            {
                ViewData["Message"] = "Произошла ошибка при загрузке файла";
                return View("DocumentAddition");
            }
        }

        private ActionResult GetDocumentInternal(int id, bool secured)
        {
            var doc = _context.Documents.FirstOrDefault(x => x.Id == id);
            if (doc.IsSecured != secured)
            {
                return NotFound();
            }
            if (doc != null)
            {
                try
                {
                    return PhysicalFile(_appEnvironment.ContentRootPath + doc.Path, "application/pdf", doc.Name + ".pdf");
                }
                catch (Exception e)
                {
                    return NotFound();
                }
            }
            return NotFound();
        }
        
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}