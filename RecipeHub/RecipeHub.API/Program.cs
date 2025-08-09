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
using Npgsql;

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

// === CONNECTION STRING (Render/Supabase compat) ===
string rawConn =
    Environment.GetEnvironmentVariable("DATABASE_URL") // Render/Heroku/Supabase style (postgres://...)
    ?? builder.Configuration.GetConnectionString("DefaultConnection"); // fallback local/appsettings

if (string.IsNullOrWhiteSpace(rawConn))
{
    Console.WriteLine("CRITICAL: No connection string found (DATABASE_URL/DefaultConnection). Using fallback local.");
    rawConn = "Host=localhost;Database=fallback;Username=postgres;Password=password";
}

static string BuildNpgsqlConnectionString(string value)
{
    if (value.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(value);

        // user:password (podem conter caracteres especiais)
        var userInfo = uri.UserInfo.Split(':', 2);
        var user = Uri.UnescapeDataString(userInfo[0]);
        var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : "";

        var csb = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Username = user,
            Password = pass,
            Database = uri.AbsolutePath.TrimStart('/'),

            // Render/Supabase geralmente exigem TLS
            SslMode = SslMode.Require,
            TrustServerCertificate = true
        };

        return csb.ToString();
    }

    // Já está no formato "Host=...;Username=...;Password=...;Database=..."
    return value;
}

var npgsqlConn = BuildNpgsqlConnectionString(rawConn);
Console.WriteLine($"DbContext using connection string length: {npgsqlConn.Length}");

// Opcional: deixar acessível via Configuration (se algo no projeto ler ConnectionStrings:DefaultConnection)
builder.Configuration["ConnectionStrings:DefaultConnection"] = npgsqlConn;

// Registra o DbContext
builder.Services.AddDbContext<RecipeDbContext>(options =>
{
    options.UseNpgsql(npgsqlConn)
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
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
                ?? jwtSettings["SecretKey"]
                ?? "your-super-secret-key-for-jwt-token-generation-recipe-hub-api";
var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
             ?? jwtSettings["Issuer"]
             ?? "RecipeHubAPI";
var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
               ?? jwtSettings["Audience"]
               ?? "RecipeHubUsers";

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

// Em Render normalmente já há TLS; mantenha para ambiente local também
app.UseHttpsRedirection();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
