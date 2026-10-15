using Microsoft.EntityFrameworkCore;
using RaceDay_POE_API.Data;
using RaceDay_POE_API.Services;

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

// Password hashing service
builder.Services.AddScoped<IPasswordService, PasswordService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "RaceDay API",
        Version = "v1",
        Description = "RESTful API for the RaceDay event platform. Log in through api/auth/login first, the session cookie is then sent with every request. Organiser and Participant roles are checked on each protected endpoint."
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "RaceDay API v1");
        options.DocumentTitle = "RaceDay API";
    });
}

app.UseHttpsRedirection();

// Session must run before the controllers so they can read it
app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();
