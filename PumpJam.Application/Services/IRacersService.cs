using Domain.Models;
using Domain.ViewModels;

namespace PumpJam.Application.Services
{
    public interface IRacersService
    {
        public Task<List<RacerDto>?> GetCurrentRace();
        public Task<List<RacerDto>?> GetCurrentQueue();
        public Task<RacerDto?> GetCurrentRacerData();
        public Task<List<Category>> GetCategoryList();
    }
}
