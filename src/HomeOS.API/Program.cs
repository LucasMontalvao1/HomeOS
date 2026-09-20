using HomeOS.API.Infrastructure.Database;
using HomeOS.API.Middlewares;
using HomeOS.API.Modules.Household.Application;
using HomeOS.API.Modules.Household.Endpoints;
using HomeOS.API.Modules.Household.Infrastructure;
using HomeOS.API.Modules.Identity.Application;
using HomeOS.API.Modules.Identity.Endpoints;
using HomeOS.API.Modules.Identity.Infrastructure;
using HomeOS.API.Modules.Products.Application;
using HomeOS.API.Modules.Products.Endpoints;
using HomeOS.API.Modules.Products.Infrastructure;
using HomeOS.API.Modules.Shopping.Application;
using HomeOS.API.Modules.Shopping.Endpoints;
using HomeOS.API.Modules.Shopping.Infrastructure;
using HomeOS.API.Modules.Telegram.Application;
using HomeOS.API.Modules.Telegram.Configuration;
using HomeOS.API.Modules.Telegram.Endpoints;
using HomeOS.API.Modules.Telegram.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Serilog;
using System.Data;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ───────────────────────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// ─── Database Migrations ───────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionString 'DefaultConnection' não configurada.");

builder.Services.AddDatabaseMigrations(connectionString);

// ─── Dapper / Npgsql ────────────────────────────────────────────────────────
builder.Services.AddTransient<IDbConnection>(_ => new NpgsqlConnection(connectionString));

// ─── JWT Authentication ─────────────────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddAuthorization();

// ─── Global Error Handler ────────────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ─── Swagger ─────────────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ─── Health Checks ────────────────────────────────────────────────────────────
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgres");

// ─── Module Services (Identity) ───────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<AuthService>();

// ─── Module Services (Household) ─────────────────────────────────────────────
builder.Services.AddScoped<IHouseholdRepository, HouseholdRepository>();
builder.Services.AddScoped<HouseholdService>();

// ─── Module Services (Products) ──────────────────────────────────────────────────
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ProductService>();

// ─── Module Services (Shopping) ──────────────────────────────────────────────────
builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.AddScoped<ShoppingListService>();

// ─── Module Services (Telegram) ──────────────────────────────────────────────────
builder.Services.Configure<TelegramSettings>(builder.Configuration.GetSection(TelegramSettings.Section));
builder.Services.AddScoped<ITelegramLinkRepository, TelegramLinkRepository>();
builder.Services.AddHttpClient<TelegramService>(); // gerencia HttpClient + ciclo de vida do TelegramService

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ─── Health Check ─────────────────────────────────────────────────────────────
app.MapHealthChecks("/health");

// ─── Module Endpoints ─────────────────────────────────────────────────────────
app.MapIdentityEndpoints();
app.MapHouseholdEndpoints();
app.MapProductEndpoints();
app.MapShoppingListEndpoints();
app.MapTelegramEndpoints();

app.MapGet("/", () => "HomeOS API is running.").AllowAnonymous();

app.Run();
