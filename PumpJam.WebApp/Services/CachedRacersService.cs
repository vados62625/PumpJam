using Domain.Models;
using Domain.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using PumpJam.Application.Services;
using PumpJam.CacheExtensions;

namespace PumpJam.Services
{
    public class CachedRacersService : IRacersService
    {
        private readonly IRacersService _racersService;
        private readonly IMemoryCache _cache;
        private const string KeyPrefix = "racers";

        public CachedRacersService(IRacersService racersService, IMemoryCache cache)
        {
            _racersService = racersService;
            _cache = cache;
        }
        public Task<List<RacerDto>?> GetCurrentRace()
        {
            return _cache.Remember($"{KeyPrefix}:list", async () => await _racersService.GetCurrentRace());
        }

        public Task<List<RacerDto>?> GetLastRaceByCategory(int id)
        {
            return _cache.Remember($"{KeyPrefix}:lastRace{id}", async () => await _racersService.GetLastRaceByCategory(id));
        }

        public Task<RacerDto?> GetCurrentRacerData()
        {
            return _cache.Remember($"{KeyPrefix}:current", async () => await _racersService.GetCurrentRacerData());            
        }

        public Task<List<Category>> GetCategoryList()
        {
            return _cache.Remember($"{KeyPrefix}:categories", async () => await _racersService.GetCategoryList());                        
        }

        public Task<List<RacerDto>?> GetCurrentQueue()
        {
            return _cache.Remember($"{KeyPrefix}:queue", async () => await _racersService.GetCurrentQueue());                        
        }

        public Task<List<RacerDto>?> GetWinners()
        {
            return _cache.Remember($"{KeyPrefix}:winners", async () => await _racersService.GetWinners());                        
        }

        public Task<List<RacerDto>?> GetWinnerByCategory(string name)
        {
            return _cache.Remember($"{KeyPrefix}:winner{name}", async () => await _racersService.GetWinnerByCategory(name));                        
        }

        public Task<List<RacerDto>?> GetPassedRacers()
        {
            return _cache.Remember($"{KeyPrefix}:passed", async () => await _racersService.GetPassedRacers());                        
        }

        public Task<List<RacerDto>?> GetPassedRacersByCategory(string name)
        {
            return _cache.Remember($"{KeyPrefix}:passedByCategory{name}", async () => await _racersService.GetPassedRacersByCategory(name));                        
            throw new NotImplementedException();
        }

        public Task<List<List<RacerDto>?>?> GetLastRaces()
        {
            return _cache.Remember($"{KeyPrefix}:lastRaces", async () => await _racersService.GetLastRaces());                        
        }
    }
}
