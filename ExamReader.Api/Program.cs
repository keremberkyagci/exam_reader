using ExamReader.Api.Repositories;
using ExamReader.Api.Services;
using Npgsql;
using StackExchange.Redis;
using System.Data;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Ortam değişkenlerinden CORS adresini oku (Yoksa varsayılan localhost:4200)
var allowedCorsOrigin = builder.Configuration["ALLOWED_CORS_ORIGIN"] ?? "http://localhost:4200";

// CORS — Angular dev server ve Canlı Sunucu
builder.Services.AddCors(options =>
{
    options.AddPolicy("Angular", policy =>
        policy.WithOrigins(allowedCorsOrigin)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Controllers
builder.Services.AddControllers();

// PostgreSQL (Dapper)
builder.Services.AddScoped<IDbConnection>(_ =>
    new NpgsqlConnection(builder.Configuration.GetConnectionString("PostgreSql")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

// Repositories & Services
builder.Services.AddScoped<IExamRepository, ExamRepository>();
builder.Services.AddScoped<IExamService, ExamService>();

builder.Services.AddScoped<IExamResultRepository, ExamResultRepository>();
builder.Services.AddScoped<IExamResultService, ExamResultService>();

builder.Services.AddScoped<IOcrService, OcrService>();

var app = builder.Build();

app.UseCors("Angular");

app.UseAuthorization();

app.MapControllers();

app.Run();
