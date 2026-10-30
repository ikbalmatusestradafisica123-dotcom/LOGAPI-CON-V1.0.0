using LOGAPI_CON.Data;
using LOGAPI_CON.Models;
using LOGAPI_CON.Security;
using LOGAPI_CON.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDevelopment", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://construpac.com", "https://www.construpac.com")
        .AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options => 
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<RoutePlanningService>();
builder.Services.AddHttpClient<IMapsService, GoogleMapsService>();
builder.Services.AddScoped<OrderStatusService>();

builder.Services.AddScoped<IOrderRepository, SqlOrderRepository>();
builder.Services.AddScoped<IVehicleRepository, SqlVehicleRepository>();

builder.Services.AddScoped<IAuthUserRepository, SqlAuthUserRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IPasswordHasher<AuthUser>, PasswordHasher<AuthUser>>();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });



builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Autenticación mediante token JWT."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        });
});







var issuer = builder.Configuration["Auth:Jwt:Issuer"];
var audience = builder.Configuration["Auth:Jwt:Audience"];
var signingKey = builder.Configuration["Auth:Jwt:SigningKey"];

if(string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience) || string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException("Falta la configuracion de JWT en User secrets o en la configuracion de retorno.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = issuer,

        ValidateAudience = true,
        ValidAudience = audience,

        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),

        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromSeconds(30),

        RoleClaimType = System.Security.Claims.ClaimTypes.Role
    };

});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(AppPolicies.CanPlanRoutes, policy => policy.RequireRole(
            AppRoles.Administrator, AppRoles.Owner, AppRoles.Supervisor, AppRoles.IT, AppRoles.Logistics));

    options.AddPolicy(AppPolicies.CanManageUsers, policy => policy.RequireRole(
            AppRoles.Administrator, AppRoles.IT, AppRoles.Owner));

    options.AddPolicy(AppPolicies.CanAuthorizeExceptions, policy => policy.RequireRole(
            AppRoles.Administrator, AppRoles.Owner));

    options.AddPolicy(AppPolicies.CanCorrectOrderStatus, policy => policy.RequireRole(
            AppRoles.Administrator, AppRoles.IT));
});


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();

    var configuration = scope.ServiceProvider
        .GetRequiredService<IConfiguration>();

    var passwordHasher = scope.ServiceProvider
        .GetRequiredService<IPasswordHasher<AuthUser>>();

    await db.Database.MigrateAsync();

    var hasUsers = await db.AuthUsers.AnyAsync();

    if (!hasUsers)
    {
        var username =
            configuration["Auth:BootstrapUser:Username"];

        var password =
            configuration["Auth:BootstrapUser:Password"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Faltan las credenciales del usuario IT inicial.");
        }

        var bootstrapUser = new AuthUser
        {
            Id = Guid.NewGuid().ToString("N"),
            Username = username.Trim(),
            Role = AppRoles.IT,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };

        bootstrapUser.PasswordHash =
            passwordHasher.HashPassword(
                bootstrapUser,
                password);

        db.AuthUsers.Add(bootstrapUser);

        await db.SaveChangesAsync();
    }
}




if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

app.UseHttpsRedirection();

app.UseCors("FrontendDevelopment");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
