using Microsoft.AspNetCore.Authentication.JwtBearer;







using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.AspNetCore;
using SnipeIt.Api.Endpoints;
using SnipeIt.Api.Models.Entities;
using System.Text;
using AutoMapper;
using SnipeIt.Api.Mappings;
using SnipeIt.Api.Services;
using SnipeIt.Api.Common;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Add FluentValidation — register all validators in this assembly so auto-validation runs them
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Configure EF Core with Pomelo MySQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "server=127.0.0.1;port=3306;database=snipeit;user=snipeit;password=changeme1234";
builder.Services.AddDbContext<SnipeitContext>(options =>
    options.UseMySql(
        connectionString,
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()));

// Configure Authentication (JWT)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "ThisIsASuperSecretKeyForSnipeItApi123!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SnipeItApi",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SnipeItUi",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// Shared cross-cutting infrastructure
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IExternalAuthProvider, NullExternalAuthProvider>();

// Add Services
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Reference data services
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IStatusLabelService, StatusLabelService>();
builder.Services.AddScoped<ILocationService, LocationService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policyBuilder =>
        {
            policyBuilder.SetIsOriginAllowed(_ => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
});

var app = builder.Build();

// Global exception handling — first in the pipeline so it wraps all downstream errors.
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

// Map Endpoints
app.MapAuthEndpoints();

app.MapControllers();

// Seed a test user if none exists (development only)
using (var scope = app.Services.CreateScope())
{
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    if (env.IsDevelopment())
    {
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<SnipeitContext>();
            // create DB if not exists
            db.Database.EnsureCreated();

            var existing = await db.Users.FirstOrDefaultAsync(u => u.Username == "testuser");
            if (existing == null)
            {
                var password = configuration["Seed:TestUserPassword"] ?? "Test123!";
                var hashed = BCrypt.Net.BCrypt.HashPassword(password);

                var user = new SnipeIt.Api.Models.Entities.User
                {
                    Username = "testuser",
                    Email = "testuser@example.com",
                    Password = hashed,
                    Activated = true,
                    DisplayName = "Test User",
                    CreatedAt = DateTime.UtcNow
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Error seeding test user");
        }
    }
}

app.Run();
