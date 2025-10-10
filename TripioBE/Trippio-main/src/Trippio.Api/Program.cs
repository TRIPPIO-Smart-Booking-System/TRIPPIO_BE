using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using Trippio.Api;
using Trippio.Api.Authorization;
using Trippio.Api.Filters;
using Trippio.Api.Idempotency;
using Trippio.Api.Service;
using Trippio.Core.ConfigOptions;
using Trippio.Core.Domain.Identity;
using Trippio.Core.Repositories;
using Trippio.Core.SeedWorks;
using Trippio.Core.Services;
using Trippio.Data;
using Trippio.Data.Repositories;
using Trippio.Data.SeedWorks;
using Trippio.Data.Service;
internal class Program
{
    private static async Task Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

        try
        {
            Log.Information("Starting web host");

            var builder = WebApplication.CreateBuilder(args);
            var configuration = builder.Configuration;
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var VietokemanPolicy = "VietokemanPolicy";

            builder.Host.UseSerilog();

            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            builder.Services.AddCors(o => o.AddPolicy(VietokemanPolicy, policy =>
            {
                var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
                else
                {
                    // Fallback for development
                    policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            }));


            builder.Services.AddDbContext<TrippioDbContext>(options =>
                options.UseSqlServer(connectionString));

            builder.Services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<TrippioDbContext>()
            .AddDefaultTokenProviders();
            //if (config["Repository:Provider"] == "SqlServer")
            //{
            //    services.AddScoped<IRepositoryFactory, SqlServerRepositoryFactory>();
            //}
            //else if (config["Repository:Provider"] == "Postgre")
            //{
            //    services.AddScoped<IRepositoryFactory, PostgreRepositoryFactory>();
            //}
            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
                options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
                options.User.RequireUniqueEmail = false;
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));

            // Register Repositories
            builder.Services.AddScoped<Trippio.Core.Repositories.IBookingRepository, Trippio.Data.Repositories.BookingRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IPaymentRepository, Trippio.Data.Repositories.PaymentRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IExtraServiceRepository, Trippio.Data.Repositories.ExtraServiceRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IFeedbackRepository, Trippio.Data.Repositories.FeedbackRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.ICommentRepository, Trippio.Data.Repositories.CommentRepository>();
            
            // Register Master Data Repositories
            builder.Services.AddScoped<Trippio.Core.Repositories.IHotelRepository, Trippio.Data.Repositories.HotelRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IRoomRepository, Trippio.Data.Repositories.RoomRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.ITransportRepository, Trippio.Data.Repositories.TransportRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.ITransportTripRepository, Trippio.Data.Repositories.TransportTripRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IShowRepository, Trippio.Data.Repositories.ShowRepository>();

            // Register Services
            //builder.Services.AddScoped<Trippio.Core.Services.IOrderService, Trippio.Data.Services.OrderService>();
            //builder.Services.AddScoped<Trippio.Core.Services.IBookingService, Trippio.Data.Services.BookingService>();
            //builder.Services.AddScoped<Trippio.Core.Services.IPaymentService, Trippio.Data.Services.PaymentService>();
            //builder.Services.AddScoped<Trippio.Core.Services.IBasketService, Trippio.Data.Services.BasketService>();
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

            
            builder.Services.AddAutoMapper(typeof(Trippio.Core.Mappings.AutoMapping));

            builder.Services.Configure<JwtTokenSettings>(configuration.GetSection("JwtTokenSettings"));
            builder.Services.Configure<MediaSettings>(configuration.GetSection("MediaSettings"));
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IBasketService, Trippio.Data.Service.BasketService>();
            // Register Email Service
            builder.Services.AddScoped<Trippio.Core.Services.IEmailService, Trippio.Data.Service.EmailService>();
            //Order
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            //
            builder.Services.AddScoped<IOrderService, Trippio.Data.Service.OrderService>();
            builder.Services.AddScoped<IBookingService, Trippio.Data.Service.BookingService>();
            //Payment
            builder.Services.AddScoped<IPaymentService, Trippio.Data.Service.PaymentService>();
            
            // Register Master Data Services
            builder.Services.AddScoped<Trippio.Core.Services.IHotelService, Trippio.Data.Services.HotelService>();
            builder.Services.AddScoped<Trippio.Core.Services.IRoomService, Trippio.Data.Services.RoomService>();
            builder.Services.AddScoped<Trippio.Core.Services.ITransportService, Trippio.Data.Services.TransportService>();
            builder.Services.AddScoped<Trippio.Core.Services.ITransportTripService, Trippio.Data.Services.TransportTripService>();
            builder.Services.AddScoped<Trippio.Core.Services.IShowService, Trippio.Data.Services.ShowService>();
            //paymentwebhook
            builder.Services.AddScoped<IIdempotencyStore, RedisIdempotencyStore>();
            //Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));

            // Add Health Checks

            builder.Services.AddHealthChecks()
                .AddSqlServer(connectionString, name: "sql-server")
                .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomOperationIds(apiDesc =>
                {
                    return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
                });
                c.SwaggerDoc("TrippioAPI", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API for Trippio",
                    Description = "API for Trippio core domain. This domain keeps track of campaigns, campaign rules, and campaign execution."
                });
                c.ParameterFilter<SwaggerNullableParameterFilter>();
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: Bearer {token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = JwtBearerDefaults.AuthenticationScheme
                    }
                },
                new string[] {}
            }
                });
            });

            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["JwtTokenSettings:Issuer"],
                    ValidAudience = configuration["JwtTokenSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtTokenSettings:Key"]))
                };
            });


            var app = builder.Build();

            app.UseStaticFiles();
            app.UseSerilogRequestLogging(); // Log HTTP request pipeline

            // CORS must be placed after UseStaticFiles and before UseAuthentication
            app.UseCors(VietokemanPolicy);

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/TrippioAPI/swagger.json", "Trippio API");
                    c.RoutePrefix = "swagger"; // This makes Swagger UI available at /swagger
                    c.DisplayOperationId();
                    c.DisplayRequestDuration();
                    c.InjectStylesheet("/swagger-custom.css");
                });
            }


            // Health check endpoints
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.MigrateDatabaseAsync();
            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}