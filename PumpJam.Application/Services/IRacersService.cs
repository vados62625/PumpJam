using Domain.Models;
using Domain.ViewModels;

namespace PumpJam.Application.Services
{
    public interface IRacersService
    {
        public Task<List<RacerDto>?> GetCurrentRace();
        public Task<List<RacerDto>?> GetCurrentQueue();
        public Task<List<RacerDto>?> GetWinners();
        public Task<List<RacerDto>?> GetWinnerByCategory(string name);
        public Task<List<RacerDto>?> GetPassedRacers();
        public Task<List<RacerDto>?> GetPassedRacersByCategory(string name);
        public Task<List<List<RacerDto>?>?> GetLastRaces();
        public Task<List<RacerDto>?> GetLastRaceByCategory(int id);
        public Task<RacerDto?> GetCurrentRacerData();
        public Task<List<Category>> GetCategoryList();
    }
}
