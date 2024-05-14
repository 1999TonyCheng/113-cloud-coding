using AspNet8Mvc_Tallybook.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AspNet8Mvc_Tallybook.Controllers
{
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public UploadController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Index1()
        {
            var files = GetUploadedFiles();
            var model = new UploadModel { Files = files };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(List<IFormFile> files)
        {
            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    var folderPath = Path.Combine(_environment.WebRootPath, "NewFloder");
                    var filePath = Path.Combine(folderPath, file.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }
            }

            return RedirectToAction("Index1");
        }

        [HttpGet]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(_environment.WebRootPath, "NewFolder", fileName);
            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/octet-stream", fileName);
            }

            return NotFound();
        }

        private List<IFormFile> GetUploadedFiles()
        {
            var folderPath = Path.Combine(_environment.WebRootPath, "NewFloder");
            var fileNames = Directory.GetFiles(folderPath).Select(fileName =>
            {
                var filePath = Path.Combine(folderPath, fileName);
                var fileStream = new FileStream(filePath, FileMode.Open);
                var file = new FormFile(fileStream, 0, fileStream.Length, null, Path.GetFileName(fileStream.Name))
                {
                    Headers = new HeaderDictionary(),
                    ContentType = "application/octet-stream"
                } as IFormFile; // 強制轉換為 IFormFile 介面;
                return file;
            }).ToList();

            return fileNames;
        }

    }
}
