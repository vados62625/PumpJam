using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PumpJam.Application.DbContext;
using PumpJam.Application.Services;
using PumpJam.DB;
using PumpJam.Repository;
using PumpJam.Services;
using PumpJam.Services.Static;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddScoped<IRacersService, RacersService>();
builder.Services.AddHttpClient<IRacersService, RacersService>()
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));
builder.Services.Decorate<IRacersService, CachedRacersService>();

builder.Services.AddScoped<RacersRepository>();

builder.Services.AddSingleton<RacersQueue>();

builder.Services.AddControllersWithViews();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<IRacersContext, RacersContext>(ConfigureUserContextConnection);

builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

void ConfigureUserContextConnection(DbContextOptionsBuilder options)
{
    options.UseLazyLoadingProxies()
        .UseSqlServer(builder.Configuration.GetConnectionString("DevContext")).ConfigureWarnings(w => w.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning));
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RacersContext>();
    db.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
