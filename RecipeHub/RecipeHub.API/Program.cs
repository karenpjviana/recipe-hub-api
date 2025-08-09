using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.WebUtilities;
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

// === CONTROLLERS + SWAGGER ===
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
        Description = "JWT Authorization header. Ex.: \"Authorization: Bearer {token}\""
    });
    c.AddSecurityRequirement(new()
    {
        {
            new() { Reference = new() { Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
});

// === INTERCEPTORS ===
builder.Services.AddScoped<SoftDeleteInterceptor>();

// === CONNECTION STRING (Render/Supabase compat) ===
string rawConn =
    Environment.GetEnvironmentVariable("DATABASE_URL") // postgresql://user:pass@host:port/db?...
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "";

static string ToNpgsqlConn(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return value;

    var trimmed = value.Trim();

    // cobre "postgres://" e "postgresql://"
    if (trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
        trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(trimmed);

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
            SslMode = SslMode.Require // TLS via Render proxy/DB
        };

        // aplica parâmetros da querystring, se houver (?sslmode=... etc.)
        var query = QueryHelpers.ParseQuery(uri.Query);
        foreach (var kv in query)
        {
            var key = kv.Key;
            var val = kv.Value.ToString();
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
                csb[key] = val;
        }

        return csb.ToString();
    }

    // Já está em pares chave=valor
    return trimmed;
}

var npgsqlConn = ToNpgsqlConn(rawConn);

// log de diagnóstico sem expor segredos
Console.WriteLine($"Conn prefix: {(string.IsNullOrEmpty(npgsqlConn) ? "<empty>" : npgsqlConn.Substring(0, Math.Min(5, npgsqlConn.Length)))}"); // esperado: "Host="

if (string.IsNullOrWhiteSpace(npgsqlConn))
{
    Console.WriteLine("CRITICAL: No connection string found. Using fallback local.");
    npgsqlConn = "Host=localhost;Database=fallback;Username=postgres;Password=password";
}

// mantém disponível se algo do projeto ler ConnectionStrings:DefaultConnection
builder.Configuration["ConnectionStrings:DefaultConnection"] = npgsqlConn;

// === DbContext ===
builder.Services.AddDbContext<RecipeDbContext>(options =>
{
    options.UseNpgsql(npgsqlConn)
           .AddInterceptors(new SoftDeleteInterceptor());
});

// === REPOSITORIES ===
builder.Services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// === APPLICATION SERVICES ===
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IImageUploadService, DatabaseImageUploadService>();

// === JWT AUTH ===
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

// === CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// === FORWARDED HEADERS (Render proxy) ===
builder.Services.Configure<ForwardedHeadersOptions>(opts =>
{
    opts.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    opts.KnownNetworks.Clear();
    opts.KnownProxies.Clear();
});

// === BUILD APP ===
var app = builder.Build();

// === MIGRAÇÃO AUTOMÁTICA PARA PRODUÇÃO ===
if (app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<RecipeDbContext>();
        try
        {
            Console.WriteLine("🔄 Executando migrations...");
            dbContext.Database.Migrate();
            Console.WriteLine("✅ Migrations executadas com sucesso!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro nas migrations: {ex.Message}");
        }
    }
}

// Swagger só em Dev (Render = Production)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// aceitar host/scheme do proxy
app.UseForwardedHeaders();

// Em produção (Render) NÃO redireciona pra HTTPS (o proxy já faz TLS)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
