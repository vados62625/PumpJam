using Domain.Models;
using Microsoft.EntityFrameworkCore;
using PumpJam.Application.DbContext;

namespace PumpJam.DB
{
    public class RacersContext : DbContext, IRacersContext
    {
        public RacersContext(DbContextOptions<RacersContext> options) : base(options) { }
        public DbSet<RacerDB> Racers { get; set; }
        public DbSet<Category> Categories { get; set; }
    }
}
