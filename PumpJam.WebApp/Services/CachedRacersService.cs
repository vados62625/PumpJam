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
        public Task<List<RacerModel>?> GetCurrentRace()
        {
            return _cache.Remember($"{KeyPrefix}:list", async () => await _racersService.GetCurrentRace());
        }

        public Task<RacerModel?> GetCurrentRacerData()
        {
            return _cache.Remember($"{KeyPrefix}:current", async () => await _racersService.GetCurrentRacerData());            
        }

        public Task<List<Category>?> GetCategoryList()
        {
            return _cache.Remember($"{KeyPrefix}:categories", async () => await _racersService.GetCategoryList());                        
        }

        public Task<List<RacerModel>?> GetCurrentQueue()
        {
            return _cache.Remember($"{KeyPrefix}:queue", async () => await _racersService.GetCurrentQueue());                        
        }
    }
}
