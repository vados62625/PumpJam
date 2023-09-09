using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using PumpJam.DB;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Web;
using Domain.Models;
using Domain.Models.CSV;
using Domain.ViewModels;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> LoadRacers(IFormFile racers)
        {
            if (racers != null)
            {
                string fileName = $"{Guid.NewGuid()}{Path.GetExtension(racers.FileName)}";
                try
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        racers.CopyTo(stream);
                        //await ReadAndSaveCategoriesFromCSV(stream);
                    }
                    await ReadAndSaveRacersFromCSV(filePath);
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
                        // foreach (var cat in categoriesDB)
                        // {
                        //     categoriesDB.Remove(cat);
                        // }
                        foreach (var details in result)
                        {
                            var category = categoriesDB.FirstOrDefault(c => c.Name == details.Category);
                            if (category == null)
                            {
                                category = new Category
                                {
                                    Name = details.Category,
                                };
                            }

                            category.Race3 = details.R3 ?? 0;
                            category.Race4 = details.R4 ?? 0;
                            category.Race5 = details.R5 ?? 0;
                            
                            categoriesDB.Update(category);
                        }
                        await context.SaveChangesAsync();
                    }
                }
            }
        }
        private async Task ReadAndSaveRacersFromCSV(string filePath)
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
                    var result = csv.GetRecords<RacerCSV>();
                    using (var context = _serviceProvider.GetRequiredService<RacersContext>())
                    {
                        var racersDb = context.Racers;
                        
                        foreach (var rac in racersDb)
                        {
                            racersDb.Remove(rac);
                        }
                        
                        foreach (var details in result)
                        {
                            var racerDb = new RacerDB
                            {
                                Name = $"{details.LastName} {details.FirstName} {details.Surname}",
                                Bib = details.Number
                            };
                            
                            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == details.Category);
                            
                            if (category == null)
                            {
                                var categoryDb = await context.Categories.AddAsync(new Category()
                                {
                                    Name = details.Category
                                });

                                racerDb.Category = categoryDb.Entity;
                            }
                            else
                            {
                                racerDb.Category = category;
                            }
                            
                            racersDb.Add(racerDb);
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
        
        [HttpGet("lastRaceView/{id}")]
        public async Task<IActionResult> LastRace(int id)
        {
            var race = await _racersService.GetLastRaceByCategory(id);
            return View(race);
        }
        
        [HttpGet("lastRace/{category}")]
        public async Task<IActionResult> GetLastRaceData(int category)
        {
            try
            {
                var racers = await _racersService.GetLastRaceByCategory(category);
                return Json(racers);
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