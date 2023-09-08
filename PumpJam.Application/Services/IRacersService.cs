using Domain.Models;
using Domain.ViewModels;

namespace PumpJam.Application.Services
{
    public interface IRacersService
    {
        public Task<List<RacerModel>?> GetCurrentRace();
        public Task<List<RacerModel>?> GetCurrentQueue();
        public Task<RacerModel?> GetCurrentRacerData();
        public Task<List<Category>?> GetCategoryList();
    }
}
