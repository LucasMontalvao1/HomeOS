using HomeOS.API.Middlewares;
using Npgsql;
using Serilog;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Database Connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddTransient<IDbConnection>((sp) => new NpgsqlConnection(connectionString));

// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString ?? string.Empty, name: "postgres");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();

// Health check endpoint
app.MapHealthChecks("/health");

app.MapGet("/", () => "HomeOS API is running.");

app.Run();
