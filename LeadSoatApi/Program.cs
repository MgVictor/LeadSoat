using LeadSoatApi.Data;
using LeadSoatApi.Data.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Connection
builder.Services.AddDbContext<LeadSoatDbContext>(options => options.UseOracle("name = ConnectionStrings:DefaultConnection"), ServiceLifetime.Transient, ServiceLifetime.Transient);

//builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

var app = builder.Build();
//var appSettings = Confi
// app.Configuration.GetSection("AppSettings");
app.Configuration.GetSection("AppSettings").Get<Email>();
// app.Configuration.GetSection("Api").Get<Api>();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseCors("AllowFromAll");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
