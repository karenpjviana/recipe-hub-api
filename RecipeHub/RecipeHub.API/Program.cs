using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RecipeHub.Infrastructure.Data;
using RecipeHub.Domain.Interfaces;
using RecipeHub.Infrastructure.Repositories;
using RecipeHub.Application.Interfaces;
using RecipeHub.Infrastructure.Interceptors;
using RecipeHub.Application.Services;
using RecipeHub.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// === CONFIGURAÇÃO DOS SERVIÇOS ===

// Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "RecipeHub API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header. Exemplo: \"Authorization: Bearer {token}\""
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            new string[] {}
        }
    });
});

// Database & Interceptors
builder.Services.AddScoped<SoftDeleteInterceptor>();

// Configuração da string de conexão com fallback e debug mais detalhado
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"DATABASE_URL from env: {(string.IsNullOrEmpty(connectionString) ? "NULL/EMPTY" : connectionString.Substring(0, 20) + "...")}");

if (string.IsNullOrEmpty(connectionString))
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"Fallback connection string: {(string.IsNullOrEmpty(connectionString) ? "NULL/EMPTY" : "SET")}");
}

// Verificação final
if (string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine("CRITICAL ERROR: No connection string found!");
    connectionString = "Host=localhost;Database=fallback;Username=postgres;Password=password";
}

Console.WriteLine($"Final connection string length: {connectionString?.Length ?? 0}");

// Substituir configuração completamente
builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;

builder.Services.AddDbContext<RecipeDbContext>((serviceProvider, options) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var connStr = config.GetConnectionString("DefaultConnection");
    Console.WriteLine($"DbContext using connection string length: {connStr?.Length ?? 0}");
    options.UseNpgsql(connStr)
           .AddInterceptors(new SoftDeleteInterceptor());
});

// Repositories
builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Application Services
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IImageUploadService, DatabaseImageUploadService>();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? 
                jwtSettings["SecretKey"] ?? 
                "your-super-secret-key-for-jwt-token-generation-recipe-hub-api";
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
             jwtSettings["Issuer"] ?? 
             "RecipeHubAPI";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
               jwtSettings["Audience"] ?? 
               "RecipeHubUsers";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === CONFIGURAÇÃO DO PIPELINE ===

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
