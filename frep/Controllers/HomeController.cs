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
            var path = _appEnvironment.ContentRootPath + "\\files\\";
            var files =  Directory.EnumerateFiles(path);
            var filesList = new List<string>();
            foreach (var file in files)
            {
                var parts = file.Split("\\");
                filesList.Add(parts[^1]);
            }
            return View(filesList);
        }

        [Authorize]
        public IActionResult Privacy()
        {
            var path = _appEnvironment.ContentRootPath + "\\files_secured\\";
            var files =  Directory.EnumerateFiles(path);
            var filesList = new List<string>();
            foreach (var file in files)
            {
                var parts = file.Split("\\");
                filesList.Add(parts[^1]);
            }
            return View(filesList);
        }
        
        public IActionResult GetDocument(string fileName)
        {
            string path = _appEnvironment.ContentRootPath + "\\files\\" + fileName;
            try
            {
                return PhysicalFile(path, "application/pdf", fileName);
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }
        
        [Authorize]
        public IActionResult GetSecuredDocument(string fileName)
        {
            string path = _appEnvironment.ContentRootPath + "\\files_secured\\" + fileName;
            try
            {
                return PhysicalFile(path, "application/pdf", fileName);
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        
        [Authorize]
        public ActionResult AddSecureDocument()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult AddSecureDocument(IFormFile selectedFile)
        {
            var realFileName = (selectedFile.FileName.Split('\\'))[^1];
            string path = "\\files_secured\\" + realFileName;
            
            if (!realFileName.Contains("pdf"))
            {
                ViewData["Message"] = "Поддерживаются только файлы pdf";
                return View();
            }

            try
            {
                using (var fileStream = new FileStream(_appEnvironment.ContentRootPath + path, FileMode.Create))
                {
                    selectedFile.CopyTo(fileStream);
                    ViewData["Message"] = "Файл успешно загружен";
                    return View();
                }
            }
            catch (Exception e)
            {
                ViewData["Message"] = "Произошла ошибка при загрузке файла";
                return View();
            }
        }
    }
}