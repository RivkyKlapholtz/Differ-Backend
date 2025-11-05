using Hangfire;
using Hangfire.SqlServer;
using DiffSpectrumView.Repositories;
using DiffSpectrumView.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Register repositories
builder.Services.AddScoped<IDiffRepository, DiffRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();

// Register services
builder.Services.AddScoped<IDiffService, DiffService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApiComparisonService, ApiComparisonService>();
builder.Services.AddScoped<IResponseNormalizationService, ResponseNormalizationService>();

// Configure Hangfire
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

// Jobs will be created when Flapi sends requests to the API

app.Run();
