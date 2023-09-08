using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using PumpJam.DB;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Domain.Models;
using Domain.Models.CSV;
using Domain.ViewModels;
using PumpJam.Application.Services;

namespace PumpJam.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRacersService _racersService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IServiceProvider _serviceProvider;

        public HomeController(ILogger<HomeController> logger, IRacersService racersService, IWebHostEnvironment hostingEnvironment, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _racersService = racersService;
            _hostingEnvironment = hostingEnvironment;
            _serviceProvider = serviceProvider;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var model = await _racersService.GetCategoryList();
            return View(model);
        }
        [HttpGet("currentRace")]
        public IActionResult CurrentRace()
        {
            return View();
        }
        [HttpGet("racersQueue")]
        public async Task<IActionResult> RacersQueue()
        {
            try
            {
                var racerList = await _racersService.GetCurrentQueue();
                return Json(racerList);
                // return Json(racerList.Aggregate(text, (str, racer) => str += $"{racer.Bib}\n"));
            }
            catch
            {
                return Json(0);
            }
        }
        
        [HttpGet("currentRacersQueueView")]
        public async Task<IActionResult> RacersQueueView()
        {
            return View();
        }
        
        [HttpPost("setNext")]
        public async Task<IActionResult> SetNext([FromQuery] int category)
        {
            using (var context = _serviceProvider.GetRequiredService<RacersContext>())
            {
                var categoriesDB = context.Categories;
                foreach (var categoryDB in categoriesDB)
                {
                    categoryDB.Next = categoryDB.Id == category;
                    categoryDB.NextDateTime = DateTime.Now;
                }                
                await context.SaveChangesAsync();
            }
            return RedirectToAction("Index", "Home");
        }
        [HttpPost("loadCategories")]
        public async Task<IActionResult> LoadCategories(IFormFile categories)
        {
            if (categories != null)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(categories.FileName)}";
                try
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        categories.CopyTo(stream);
                        //await ReadAndSaveCategoriesFromCSV(stream);
                    }
                    await ReadAndSaveCategoriesFromCSV(filePath);
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Ошибка сохранения файла\n{ex}");
                    return BadRequest();
                }
            }
            return BadRequest();
        }
        private async Task ReadAndSaveCategoriesFromCSV(string filePath)
        {
            using (var reader = new StreamReader(filePath))
            {
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    Delimiter = ";",
                    Encoding = Encoding.UTF8,
                    
                };
                using (var csv = new CsvReader(reader, csvConfig))
                {
                    var result = csv.GetRecords<CategoryCSV>();
                    using (var context = _serviceProvider.GetRequiredService<RacersContext>())
                    {
                        var categoriesDB = context.Categories;
                        foreach (var details in result)
                        {
                            foreach (var cat in categoriesDB)
                            {
                                categoriesDB.Remove(cat);
                            }
                            categoriesDB.Add(new Category
                            {
                                Name = details.Category ?? "NoName",
                                Race3 = details.R3 ?? 0,
                                Race4 = details.R4 ?? 0,
                                Race5 = details.R5 ?? 0,
                            });
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
        [HttpGet("current")]
        public IActionResult CurrentRacer()
        {
            return View();
        }

        [HttpGet("racers")]
        public async Task<IActionResult> GetData()
        {
            try
            {
                var racerList = await _racersService.GetCurrentRace();
                return Json(racerList);
            }
            catch
            {
                return Json(0);
            }
        }
        [HttpGet("currentRacer")]
        public async Task<IActionResult> GetCurrentRacerData()
        {
            try
            {
                var racer = await _racersService.GetCurrentRacerData();
                return Json(new List<RacerDto> { racer });
            }
            catch
            {
                return Json(0);
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}