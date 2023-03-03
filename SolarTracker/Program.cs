using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


//CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(CultureInfo.CurrentCulture.Name)
//{
//    NumberFormat = new NumberFormatInfo()
//};
//var appSettings = new AppSettings
//{
//    ApiKey = string.Empty,
//    Latitude = 0,
//    Longitude = 0
//};

//var s = new AstroApiService(appSettings);
//var si = await s.GetSunInfo();


//Console.WriteLine(si);



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

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

app.UseAuthorization();

app.MapControllers();

app.Run();




