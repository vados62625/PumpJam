using Domain.Models;
using Domain.ViewModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PumpJam.Application.Repository;
using PumpJam.Application.Services;
using PumpJam.DB;
using PumpJam.Repository;
using PumpJam.Services.Static;

namespace PumpJam.Services
{
    public class RacersService : IRacersService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RacersService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RacersRepository _racersRepository;
        private readonly RacersContext _context;
        private const string Url = "https://api.raceresult.com/252165/S6MWZHENY2BJJQMOMELRGZ18T79AEZVU";
        private static List<double> _lastQueue;
        private static string _lastCategory;
        private readonly RacersQueue _racersQueue;

        public RacersService(HttpClient httpClient, ILogger<RacersService> logger, IServiceProvider serviceProvider,
            RacersContext context, RacersRepository racersRepository, RacersQueue racersQueue)
        {
            _httpClient = httpClient;
            _logger = logger;
            _serviceProvider = serviceProvider;
            _context = context;
            _racersRepository = racersRepository;
            _racersQueue = racersQueue;
        }

        public async Task<List<RacerDto>?> GetCurrentRace()
        {
            //TODO поменять json на апи
            var racersData = await GetRacersDataAsync();
            if (racersData != null)
            {
                var racersList = JsonConvert.DeserializeObject<List<Racer>>(racersData);
                if (racersList != null)
                {
                    return GetCurrentRaceData(racersList);
                }

                return null;
            }

            return null;
        }

        public async Task<List<RacerDto>?> GetLastRaceByCategory(int id)
        {
            var racersData = await GetRacersDataAsync();
            if (racersData != null)
            {
                var racersList = JsonConvert.DeserializeObject<List<Racer>>(racersData);
                if (racersList != null)
                {
                    var catName = _context.Categories.FirstOrDefault(c => c.Id == id);
                    var data = await GetLastRaceByCategoryData(racersList, catName);
                    return data.Where(c => c.Rank != null && !string.IsNullOrEmpty(c.Rank)).ToList();
                }

                return null;
            }

            return null;
        }

        public async Task<RacerDto?> GetCurrentRacerData()
        {
            var racersData = await GetRacersDataAsync();
            if (racersData != null)
            {
                var racersList = JsonConvert.DeserializeObject<List<Racer>>(racersData);
                if (racersList != null)
                {
                    return GetCurrentRacerData(racersList);
                }

                return null;
            }

            return null;
        }

        public Task<List<Category>> GetCategoryList()
        {
            return _context.Set<Category>().ToListAsync();
        }

        public async Task<List<RacerDto>?> GetCurrentQueue()
        {
            //TODO поменять json на апи
            var racersData = await GetRacersDataAsync();
            if (racersData != null)
            {
                var racersList = JsonConvert.DeserializeObject<List<Racer>>(racersData);
                if (racersList != null)
                {
                    var catName = _context.Categories.FirstOrDefault(c => c.Next);
                    if (catName == null)
                    {
                        catName = await GetCurrentCategory(racersList);
                    }

                    var data = await GetCurrentRaceData(racersList, catName);
                    var newData = data;
                    if (data != null && data.Any())
                    {
                        if (data[0].Contest == _racersQueue.LastCategory)
                            // if (data[0].Contest == _lastCategory)
                        {
                            newData = CheckQueue(data);
                        }

                        // if (_racersQueue.LastQueue == null)
                        if (_racersQueue.LastQueue == null || !_racersQueue.LastQueue.Any() ||
                            data.Select(c => c.LAST ?? 0).Except(_racersQueue.LastQueue).Any())
                        {
                            _racersQueue.LastQueue = data.Select(c => c.LAST ?? 0).ToList();
                        }

                        _racersQueue.LastCategory = data[0].Contest;
                    }

                    return newData;
                }

                return null;
            }

            return null;
        }

        public Task<List<RacerDto>?> GetWinners()
        {
            throw new NotImplementedException();
        }

        public Task<List<RacerDto>?> GetWinnerByCategory(string name)
        {
            throw new NotImplementedException();
        }

        public Task<List<RacerDto>?> GetPassedRacers()
        {
            throw new NotImplementedException();
        }

        public Task<List<RacerDto>?> GetPassedRacersByCategory(string name)
        {
            throw new NotImplementedException();
        }

        public async Task<List<List<RacerDto>?>?> GetLastRaces()
        {
            var racersData = await GetRacersDataAsync();
            if (racersData != null)
            {
                var racersList = JsonConvert.DeserializeObject<List<Racer>>(racersData);
                if (racersList != null)
                {
                    var categories = await GetCategoryList();
                    var lastRaces = new List<List<RacerDto>>();
                    foreach (var category in categories)
                    {
                        var catName = _context.Categories.FirstOrDefault(c => c.Id == category.Id);
                        var data = await GetLastRaceByCategoryData(racersList, catName);
                        lastRaces.Add(data.Where(c => c.Rank != null && !string.IsNullOrEmpty(c.Rank)).ToList());
                    }

                    return lastRaces;
                }

                return null;
            }

            return null;
        }

        private async Task<string?> GetRacersDataAsync()
        {
            var result = await _httpClient.GetAsync(Url);
            if (result != null && result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var dataString = await result.Content.ReadAsStringAsync();
                return dataString;
            }

            return null;
        }

        private async Task<string> GetRacersDataFromJsonAsync()
        {
            using var file =
                new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    "racist.json"));
            return await file.ReadToEndAsync();
        }

        private List<RacerDto>? GetCurrentRaceData(List<Racer> racers)
        {
            var races = racers.GroupBy(c => c.Contest).ToDictionary(g => g.Key, g => g.ToList());

            var currentRacer = racers.MaxBy(c => c.LAST);
            var currentRacerLastSeconds = currentRacer?.LAST ?? 0;
            var currentRacerLast = TimeSpan.FromSeconds(currentRacerLastSeconds);

            var selectedContest = _context.Categories.FirstOrDefault(c => c.Next);

            var currentRaceContest = currentRacer?.Contest;
            if (selectedContest != null && selectedContest.NextDateTime.TimeOfDay > currentRacerLast && races.ContainsKey(selectedContest.Name))
            {
                currentRaceContest = selectedContest.Name;
            }

            if (currentRaceContest != null)
            {
                var raceNumber = 1;
                var currentRacers = races[currentRaceContest];
                int racersCount = currentRacers.Count;
                if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H5t) && c.H5t != "-") != null)
                {
                    raceNumber = 5;
                    var emptyRank = currentRacers.Where(c => c.Race5r == "-" || string.IsNullOrEmpty(c.Race5r))
                        .ToList();
                    currentRacers = currentRacers.Where(c => int.TryParse(c.Race5r, out var value))
                        .OrderBy(c => int.Parse(c.Race5r)).ToList();
                    currentRacers.AddRange(emptyRank);
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H4t) && c.H4t != "-") != null)
                {
                    raceNumber = 4;
                    var emptyRank = currentRacers.Where(c => c.Race4r == "-" || string.IsNullOrEmpty(c.Race4r))
                        .ToList();
                    currentRacers = currentRacers.Where(c => int.TryParse(c.Race4r, out var value))
                        .OrderBy(c => int.Parse(c.Race4r)).ToList();
                    currentRacers.AddRange(emptyRank);
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H3t) && c.H3t != "-") != null)
                {
                    raceNumber = 3;
                    var emptyRank = currentRacers.Where(c => c.Race3r == "-" || string.IsNullOrEmpty(c.Race3r))
                        .ToList();
                    currentRacers = currentRacers.Where(c => int.TryParse(c.Race3r, out var value))
                        .OrderBy(c => int.Parse(c.Race3r)).ToList();
                    currentRacers.AddRange(emptyRank);
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H2t) && c.H2t != "-") != null)
                    raceNumber = 2;

                using (var context = _serviceProvider.GetRequiredService<RacersContext>())
                {
                    switch (raceNumber)
                    {
                        case 0:
                            racersCount = currentRacers.Count;
                            break;
                        case 3:
                            racersCount = context.Categories.FirstOrDefault(c => c.Name == currentRaceContest)?.Race3 ??
                                          currentRacers.Count;
                            break;
                        case 4:
                            racersCount = context.Categories.FirstOrDefault(c => c.Name == currentRaceContest)?.Race4 ??
                                          currentRacers.Count;
                            break;
                        case 5:
                            racersCount = context.Categories.FirstOrDefault(c => c.Name == currentRaceContest)?.Race5 ??
                                          currentRacers.Count;
                            break;
                    }
                }

                if (raceNumber < 3)
                {
                    if (currentRacers.FirstOrDefault(c => c.RankQual != "-") != null)
                    {
                        var emptyRank = currentRacers
                            .Where(c => c.RankQual == "-" || string.IsNullOrEmpty(c.RankQual)).ToList();
                        currentRacers = currentRacers.Where(c => int.TryParse(c.RankQual, out var value))
                            .OrderBy(c => int.Parse(c.RankQual)).ToList();
                        currentRacers.AddRange(emptyRank);
                    }
                    else if (currentRacers.FirstOrDefault(c => c.Race2r != null) != null)
                    {
                        var emptyRank = currentRacers.Where(c => c.Race2r == "-" || string.IsNullOrEmpty(c.Race2r))
                            .ToList();
                        currentRacers = currentRacers.Where(c => int.TryParse(c.Race2r, out var value))
                            .OrderBy(c => int.Parse(c.Race2r)).ToList();
                        currentRacers.AddRange(emptyRank);
                    }
                    else if (currentRacers.FirstOrDefault(c => c.Race1r != null) != null)
                    {
                        var emptyRank = currentRacers.Where(c => c.Race1r == "-" || string.IsNullOrEmpty(c.Race1r))
                            .ToList();
                        currentRacers = currentRacers.Where(c => int.TryParse(c.Race1r, out var value))
                            .OrderBy(c => int.Parse(c.Race1r)).ToList();
                        currentRacers.AddRange(emptyRank);
                    }

                    return currentRacers?.Take(racersCount).Select(c => new RacerDto
                    {
                        RaceNum = raceNumber,
                        Bib = c.Bib,
                        Contest = c.Contest,
                        Qual = true,
                        BEST = c.BEST,
                        LAST = c.LAST,
                        Hb = c.H1b ?? string.Empty,
                        HbClass = (c.H1b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        Hr = c.H1r ?? "--",
                        Ht = c.H1t ?? "--",
                        H2b = c.H2b ?? string.Empty,
                        H2bClass = (c.H2b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        H2t = c.H2t ?? "--",
                        H2r = c.H2r ?? "--",
                        Name = c.Name,
                        Rank = c.RankQual ?? "--",
                        RaceT = c.Race1t ?? "--",
                        Race2T = c.Race2t ?? "--",
                    }).ToList();
                }

                return currentRacers?.Take(racersCount).Select(c => new RacerDto
                {
                    RaceNum = raceNumber,
                    Bib = c.Bib,
                    Contest = c.Contest,
                    Qual = false,
                    BEST = c.BEST,
                    LAST = c.LAST,
                    Hb = c?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(c, null)?.ToString() ??
                         string.Empty,
                    HbClass = (c?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(c, null)?.ToString()
                        ?.Contains('-') ?? true)
                        ? "text-success"
                        : "text-danger",
                    Hr = c?.GetType()?.GetProperty($"H{raceNumber}r")?.GetValue(c, null)?.ToString() ?? "--",
                    Ht = c?.GetType()?.GetProperty($"H{raceNumber}t")?.GetValue(c, null)?.ToString() ?? "--",
                    Name = c.Name,
                    Rank = (c?.GetType()?.GetProperty($"Race{raceNumber}r")?.GetValue(c, null)?.ToString()) ?? "--",
                    RaceT = c?.GetType()?.GetProperty($"Race{raceNumber}t")?.GetValue(c, null)?.ToString() ?? "--",
                }).ToList();
            }

            return null;
        }

        private async Task<List<RacerDto>?> GetCurrentRaceData(List<Racer> racers, Category currentRaceContest)
        {
            var races = racers.Where(c => !string.IsNullOrEmpty(c.Contest))
                .GroupBy(c => c.Contest)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            if (currentRaceContest != null)
            {
                var raceNumber = 1;
                var currentRacers = races[currentRaceContest.Name];

                int racersCount = currentRacers.Count;
                if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H5t) && c.H5t != "-") != null)
                {
                    raceNumber = 5;
                    // currentRacers = currentRacers.Where(c => c.Race5r == "-" || string.IsNullOrEmpty(c.Race5r))
                    //     .ToList();
                    // currentRacers = currentRacers.Where(c => int.TryParse(c.Race5r, out var value))
                    //     .OrderBy(c => int.Parse(c.Race5r)).ToList();
                    // currentRacers.AddRange(emptyRank);
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H4t) && c.H4t != "-") != null)
                {
                    raceNumber = 4;
                   // currentRacers = currentRacers.Where(c => c.Race4r == "-" || string.IsNullOrEmpty(c.Race4r))
                        // .ToList();
                    // currentRacers = currentRacers.Where(c => int.TryParse(c.Race4r, out var value))
                    //     .OrderBy(c => int.Parse(c.Race4r)).ToList();
                    // currentRacers.AddRange(emptyRank);
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H3t) && c.H3t != "-") != null)
                {
                    raceNumber = 3;

                    // currentRacers = currentRacers.Where(c => c.Race3r == "-" || string.IsNullOrEmpty(c.Race3r))
                        // .ToList();
                    // currentRacers = currentRacers.Where(c => int.TryParse(c.Race3r, out var value))
                    //     .OrderBy(c => int.Parse(c.Race3r)).ToList();
                    // currentRacers.AddRange(emptyRank);
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H2t) && c.H2t != "-") != null)
                    raceNumber = 2;

                if (raceNumber == 5)
                {
                    racersCount = currentRaceContest?.Race5 ?? currentRacers.Count;
                    currentRacers = currentRacers.Where(c => !double.TryParse(c.H5t, out var value)).ToList();
                }

                if (raceNumber == 4)
                {
                    racersCount = currentRaceContest?.Race4 ?? currentRacers.Count;
                    currentRacers = currentRacers.Where(c => !double.TryParse(c.H4t, out var value) && string.IsNullOrEmpty(c.Race4r)).ToList();
                }

                if (raceNumber == 3)
                {
                    racersCount = currentRaceContest?.Race3 ?? currentRacers.Count;
                    currentRacers = currentRacers.Where(c => !double.TryParse(c.H3t, out var value) && string.IsNullOrEmpty(c.Race3r)).ToList();

                }

                if (racersCount == 0) racersCount = currentRacers.Count;

                if (raceNumber < 3)
                {
                    if (raceNumber == 2)
                    {
                        currentRacers = currentRacers.Where(c => !double.TryParse(c.H2t, out var value) && string.IsNullOrEmpty(c.Race2r)).ToList();
                    }

                    if (raceNumber == 1)
                    {
                        currentRacers = currentRacers.Where(c => !double.TryParse(c.H1t, out var value) && string.IsNullOrEmpty(c.Race1r)).ToList();
                    }

                    var res = currentRacers?.Select(c => new RacerDto
                    {
                        RaceNum = raceNumber,
                        Bib = c.Bib,
                        Contest = c.Contest,
                        Qual = true,
                        BEST = c.BEST,
                        LAST = c.LAST,
                        Hb = c.H1b ?? string.Empty,
                        HbClass = (c.H1b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        Hr = c.H1r ?? "--",
                        Ht = c.H1t ?? "--",
                        H2b = c.H2b ?? string.Empty,
                        H2bClass = (c.H2b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        H2t = c.H2t ?? "--",
                        H2r = c.H2r ?? "--",
                        Name = c.Name,
                        Rank = c.RankQual ?? "--",
                        RaceT = c.Race1t ?? "--",
                        Race2T = c.Race2t ?? "--",
                    }).ToList();

                    // res = res?.OrderBy(c => c.Id).Take(racersCount)
                    //     .ToList();

                    foreach (var r in res)
                    {
                        var rcr = await _racersRepository.First(c => c.Bib == r.Bib);
                        if (rcr != null)
                            r.Id = rcr.Id;
                    }
                    _logger.LogError("RES 1 :::::::::: " + res.Any().ToString());
                    return res.OrderBy(c => c.Id).Take(racersCount)
                        .ToList();
                    

                }


                var result = currentRacers?.Select(c => new RacerDto
                {
                    RaceNum = raceNumber,
                    Bib = c.Bib,
                    Contest = c.Contest,
                    Qual = false,
                    BEST = c.BEST,
                    LAST = c.LAST,
                    Hb = c?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(c, null)?.ToString() ??
                         string.Empty,
                    HbClass = (c?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(c, null)?.ToString()
                        ?.Contains('-') ?? true)
                        ? "text-success"
                        : "text-danger",
                    Hr = c?.GetType()?.GetProperty($"H{raceNumber}r")?.GetValue(c, null)?.ToString() ?? "--",
                    Ht = c?.GetType()?.GetProperty($"H{raceNumber}t")?.GetValue(c, null)?.ToString() ?? "--",
                    Name = c.Name,
                    Rank = (c?.GetType()?.GetProperty($"Race{raceNumber - 1}r")?.GetValue(c, null)
                        ?.ToString()) ?? "--",
                    RaceT = c?.GetType()?.GetProperty($"Race{raceNumber}t")?.GetValue(c, null)?.ToString() ??
                            "--",
                    //}).Where(c => c.Ht != "--" && !string.IsNullOrEmpty(c.Ht))
                }).ToList();

                foreach (var r in result)
                {
                    var rcr = await _racersRepository.First(c => c.Bib == r.Bib);
                    if (rcr != null)
                        r.Id = rcr.Id;
                }

                return result.OrderBy(c => c.Id)
                    .Take(racersCount)
                    .ToList();
            }

            return null;
        }

        private async Task<List<RacerDto>?> GetLastRaceByCategoryData(List<Racer> racers, Category currentRaceContest)
        {
            var raceNumber = 1;
            var currentRacers = racers.Where(c => c.Contest == currentRaceContest.Name).ToList();
            var racersCount = currentRacers.Count;
            if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H5t) && c.H5t != "-") != null)
            {
                raceNumber = 5;
                var emptyRank = currentRacers.Where(c => c.Race5r == "-" || string.IsNullOrEmpty(c.Race5r))
                    .ToList();
                currentRacers = currentRacers.Where(c => int.TryParse(c.Race5r, out var value))
                    .OrderBy(c => int.Parse(c.Race5r)).ToList();
                currentRacers.AddRange(emptyRank);
            }
            else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H4t) && c.H4t != "-") != null)
            {
                raceNumber = 4;
                var emptyRank = currentRacers.Where(c => c.Race4r == "-" || string.IsNullOrEmpty(c.Race4r))
                    .ToList();
                currentRacers = currentRacers.Where(c => int.TryParse(c.Race4r, out var value))
                    .OrderBy(c => int.Parse(c.Race4r)).ToList();
                currentRacers.AddRange(emptyRank);
            }
            else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H3t) && c.H3t != "-") != null)
            {
                raceNumber = 3;
                var emptyRank = currentRacers.Where(c => c.Race3r == "-" || string.IsNullOrEmpty(c.Race3r))
                    .ToList();
                currentRacers = currentRacers.Where(c => int.TryParse(c.Race3r, out var value))
                    .OrderBy(c => int.Parse(c.Race3r)).ToList();
                currentRacers.AddRange(emptyRank);
            }
            else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H2t) && c.H2t != "-") != null)
                raceNumber = 2;

            if (raceNumber == 5)
            {
                racersCount = currentRaceContest?.Race5 ?? currentRacers.Count;
                currentRacers = currentRacers.Where(c => !double.TryParse(c.H5t, out var value)).ToList();
            }

            if (raceNumber == 4)
            {
                racersCount = currentRaceContest?.Race4 ?? currentRacers.Count;
                currentRacers = currentRacers.Where(c => !double.TryParse(c.H4t, out var value)).ToList();
            }

            if (raceNumber == 3)
            {
                racersCount = currentRaceContest?.Race3 ?? currentRacers.Count;
                currentRacers = currentRacers.Where(c => !double.TryParse(c.H3t, out var value)).ToList();
            }

            if (racersCount == 0) racersCount = currentRacers.Count;

            var kickedCount = currentRacers.Count - racersCount;

            if (raceNumber < 3)
            {
                if (raceNumber == 2)
                {
                    currentRacers = currentRacers.Where(c => !double.TryParse(c.H2t, out var value)).ToList();
                }

                if (raceNumber == 1)
                {
                    currentRacers = currentRacers.Where(c => !double.TryParse(c.H1t, out var value)).ToList();
                }

                var res = currentRacers?.Select(c => new RacerDto
                    {
                        RaceNum = raceNumber,
                        Bib = c.Bib,
                        Contest = c.Contest,
                        Qual = true,
                        BEST = c.BEST,
                        LAST = c.LAST,
                        Hb = c.H1b ?? string.Empty,
                        HbClass = (c.H1b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        Hr = c.H1r ?? "--",
                        Ht = c.H1t ?? "--",
                        H2b = c.H2b ?? string.Empty,
                        H2bClass = (c.H2b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        H2t = c.H2t ?? "--",
                        H2r = c.H2r ?? "--",
                        Name = c.Name,
                        Rank = c.RankQual ?? "--",
                        RaceT = c.Race1t ?? "--",
                        Race2T = c.Race2t ?? "--",
                    }).Where(c => int.TryParse(c.Rank, out var rank))
                    .OrderBy(c => int.Parse(c.Rank))
                    .Take(racersCount)
                    .ToList();


                // res.TakeLast(kickedCount).ToList().ForEach(c => c.Kicked = true);

                foreach (var r in res)
                {
                    var rcr = await _racersRepository.First(c => c.Bib == r.Bib);
                    if (rcr != null)
                        r.Id = rcr.Id;
                }

                return res.ToList();
            }

            var result = currentRacers?.Select(c => new RacerDto
                {
                    RaceNum = raceNumber,
                    Bib = c.Bib,
                    Contest = c.Contest,
                    Qual = false,
                    BEST = c.BEST,
                    LAST = c.LAST,
                    Hb = c?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(c, null)?.ToString() ??
                         string.Empty,
                    HbClass = (c?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(c, null)?.ToString()
                        ?.Contains('-') ?? true)
                        ? "text-success"
                        : "text-danger",
                    Hr = c?.GetType()?.GetProperty($"H{raceNumber}r")?.GetValue(c, null)?.ToString() ?? "--",
                    Ht = c?.GetType()?.GetProperty($"H{raceNumber}t")?.GetValue(c, null)?.ToString() ?? "--",
                    Name = c.Name,
                    Rank = (c?.GetType()?.GetProperty($"Race{raceNumber - 1}r")?.GetValue(c, null)
                        ?.ToString()) ?? "--",
                    RaceT = c?.GetType()?.GetProperty($"Race{raceNumber}t")?.GetValue(c, null)?.ToString() ??
                            "--",
                    //}).Where(c => c.Ht != "--" && !string.IsNullOrEmpty(c.Ht))
                    // }).OrderBy(c => c.Id)
                }).Where(c => int.TryParse(c.Rank, out var rank))
                .OrderBy(c => int.Parse(c.Rank))
                .Take(racersCount)
                .ToList();

            // result.TakeLast(kickedCount).ToList().ForEach(c => c.Kicked = true);

            return result;
        }

        private RacerDto? GetCurrentRacerData(List<Racer> racers)
        {
            var races = racers.GroupBy(c => c.Contest).ToDictionary(g => g.Key, g => g.ToList());
            var currentRacer = racers.OrderByDescending(c => c.LAST).FirstOrDefault();
            if (currentRacer == null) return null;

            var currentRaceContest = currentRacer.Contest;
            if (currentRaceContest != null)
            {
                var raceNumber = 1;
                var currentRacers = races[currentRaceContest];
                if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H5t) && c.H5t != "-") != null)
                {
                    raceNumber = 5;
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H4t) && c.H4t != "-") != null)
                {
                    raceNumber = 4;
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H3t) && c.H3t != "-") != null)
                {
                    raceNumber = 3;
                }
                else if (currentRacers.FirstOrDefault(c => !string.IsNullOrEmpty(c.H2t) && c.H2t != "-") != null)
                    raceNumber = 2;

                if (raceNumber < 3)
                {
                    return new RacerDto
                    {
                        RaceNum = raceNumber,
                        Bib = currentRacer.Bib,
                        Contest = currentRacer.Contest,
                        Qual = true,
                        BEST = currentRacer.BEST,
                        LAST = currentRacer.LAST,
                        Hb = currentRacer.H1b ?? string.Empty,
                        HbClass = (currentRacer.H1b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        Hr = currentRacer.H1r ?? "--",
                        Ht = currentRacer.H1t ?? "--",
                        H2b = currentRacer.H2b ?? string.Empty,
                        H2bClass = (currentRacer.H2b?.Contains('-') ?? true) ? "text-success" : "text-danger",
                        H2t = currentRacer.H2t ?? "--",
                        H2r = currentRacer.H2r ?? "--",
                        Name = currentRacer.Name,
                        Rank = currentRacer.RankQual ?? "--",
                        RaceT = currentRacer.Race1t ?? "--",
                        Race2T = currentRacer.Race2t ?? "--",
                    };
                }

                return new RacerDto
                {
                    RaceNum = raceNumber,
                    Bib = currentRacer.Bib,
                    Contest = currentRacer.Contest,
                    Qual = false,
                    BEST = currentRacer.BEST,
                    LAST = currentRacer.LAST,
                    Hb = currentRacer?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(currentRacer, null)
                        ?.ToString() ?? string.Empty,
                    HbClass = (currentRacer?.GetType()?.GetProperty($"H{raceNumber}b")?.GetValue(currentRacer, null)
                        ?.ToString()?.Contains('-') ?? true)
                        ? "text-success"
                        : "text-danger",
                    Hr = currentRacer?.GetType()?.GetProperty($"H{raceNumber}r")?.GetValue(currentRacer, null)
                        ?.ToString() ?? "--",
                    Ht = currentRacer?.GetType()?.GetProperty($"H{raceNumber}t")?.GetValue(currentRacer, null)
                        ?.ToString() ?? "--",
                    Name = currentRacer.Name,
                    Rank = (currentRacer?.GetType()?.GetProperty($"Race{raceNumber}r")?.GetValue(currentRacer, null)
                        ?.ToString()) ?? "--",
                    RaceT = currentRacer?.GetType()?.GetProperty($"Race{raceNumber}t")?.GetValue(currentRacer, null)
                        ?.ToString() ?? "--",
                };
            }

            return null;
        }

        private List<RacerDto> CheckQueue(List<RacerDto> racers)
        {
            if (_racersQueue.LastQueue == null || !_racersQueue.LastQueue.Any()) return racers;
            // for (var i = 1; i < racers.Count; i++)
            // {
            //     if (racers[i].LAST == _racersQueue.LastQueue[i]) 
            //         continue;
            //     
            //     racers.Remove(racers[i]);
            //     // RacersQueue.LastQueue.Remove(RacersQueue.LastQueue[i]);
            // }
            var current = racers.Select(c => c.LAST ?? 0);
            var times = _racersQueue.LastQueue.Intersect(current).ToList();

            return racers.Where(c => times.Contains(c.LAST ?? 0)).OrderBy(c => c.Id).ToList();
        }

        private async Task<Category?> GetCurrentCategory(List<Racer> racers)
        {
            var lastRacer = racers.MaxBy(c => c.LAST);
            var catName = lastRacer.Contest;
            if (catName != null)
            {
                return await _context.Categories.FirstOrDefaultAsync(c => c.Name == catName);
            }

            return default;
        }
    }
}