using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Connect the API to SQL Server using the connection string in appsettings.json
builder.Services.AddDbContext<RaceDayDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDayConnection")));

/*
 Session setup, the logged in user id and role are stored server side.
 The client only gets a session cookie back.
*/
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Session must run before the controllers so they can read it
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
