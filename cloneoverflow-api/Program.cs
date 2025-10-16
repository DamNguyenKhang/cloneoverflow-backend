
using Application.Services;
using Config;
using Domain;
using Domain.Interfaces;
using Infrastructure.AppDbContext;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace cloneoverflow_api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // Identity DB Context for injection
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // AppSetting.json
            var jwtSettings = builder.Configuration.GetSection("Jwt");
            var key = jwtSettings["Key"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var timeExpires = jwtSettings["AccessTokenExpireMinutes"];

            // Validate JWT settings
            if (string.IsNullOrEmpty(key) || key.Length < 32)
                throw new InvalidOperationException("JWT Key must be at least 32 characters long.");
            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
                throw new InvalidOperationException("JWT Issuer and Audience must be configured.");
            if (!int.TryParse(timeExpires, out _))
                throw new InvalidOperationException("AccessTokenExpireMinutes must be a valid integer.");

            var keyBytes = Encoding.UTF8.GetBytes(key);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(op =>
                {
                    op.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
                        ClockSkew = TimeSpan.Zero
                    };
                    op.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Cookies["accessToken"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            // DI
            builder.Services.AddSingleton<JwtUtils>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IRoleService, RoleService>();

            var app = builder.Build();


            // Init role
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await dbContext.Database.MigrateAsync();
                var roleService = scope.ServiceProvider.GetRequiredService<IRoleService>();
                await roleService.InitializeDefaultRolesAsync();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();


            app.MapControllers();

            await app.RunAsync();
        }
    }
}
