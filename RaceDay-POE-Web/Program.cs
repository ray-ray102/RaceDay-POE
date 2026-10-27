using Microsoft.Extensions.Options;
using RaceDay_POE_Web.Configuration;
using RaceDay_POE_Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// RaceDay Web API (Part 2) - the only data source for this MVC app
builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.SectionName));
builder.Services.AddHttpClient<RaceDayApiClient>((sp, client) =>
{
    var settings = sp.GetRequiredService<IOptions<ApiSettings>>().Value;
    client.BaseAddress = new Uri(settings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
