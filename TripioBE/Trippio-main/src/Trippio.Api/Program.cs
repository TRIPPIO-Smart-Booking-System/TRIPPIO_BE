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
using System.Text.Json;
using System.Text.Json.Serialization;

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

using Trippio.Core.Models.Basket; // ProductType

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

            // Authorization policies
            builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            // CORS
            builder.Services.AddCors(o => o.AddPolicy(VietokemanPolicy, policy =>
            {
                var allowedOrigins = configuration.GetSection("AllowedOrigins").Get<string[]>();
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("Content-Disposition", "X-Total-Count");
                }
                else
                {
                    // Fallback for development
                    policy.WithOrigins("http://localhost:3000", "http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithExposedHeaders("Content-Disposition", "X-Total-Count");
                }
            }));

            // DbContext
            builder.Services.AddDbContext<TrippioDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Identity
            builder.Services.AddIdentity<AppUser, AppRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
            })
            .AddEntityFrameworkStores<TrippioDbContext>()
            .AddDefaultTokenProviders();

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

            // UoW & Generic repo
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped(typeof(IRepository<,>), typeof(RepositoryBase<,>));

            // Repositories
            builder.Services.AddScoped<Trippio.Core.Repositories.IBookingRepository, Trippio.Data.Repositories.BookingRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IPaymentRepository, Trippio.Data.Repositories.PaymentRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IExtraServiceRepository, Trippio.Data.Repositories.ExtraServiceRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IFeedbackRepository, Trippio.Data.Repositories.FeedbackRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.ICommentRepository, Trippio.Data.Repositories.CommentRepository>();

            // Master Data Repositories
            builder.Services.AddScoped<Trippio.Core.Repositories.IHotelRepository, Trippio.Data.Repositories.HotelRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IRoomRepository, Trippio.Data.Repositories.RoomRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.ITransportRepository, Trippio.Data.Repositories.TransportRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.ITransportTripRepository, Trippio.Data.Repositories.TransportTripRepository>();
            builder.Services.AddScoped<Trippio.Core.Repositories.IShowRepository, Trippio.Data.Repositories.ShowRepository>();

            // AutoMapper
            builder.Services.AddAutoMapper(typeof(Trippio.Core.Mappings.AutoMapping));

            // Options
            builder.Services.Configure<JwtTokenSettings>(configuration.GetSection("JwtTokenSettings"));
            builder.Services.Configure<MediaSettings>(configuration.GetSection("MediaSettings"));
            builder.Services.Configure<RedirectUrlsOptions>(configuration.GetSection("Payments:RedirectUrls"));
            builder.Services.Configure<PayOSSettings>(configuration.GetSection("PayOS"));

            // App services
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<IBasketService, Trippio.Data.Service.BasketService>();
            builder.Services.AddScoped<Trippio.Core.Services.IEmailService, Trippio.Data.Service.EmailService>();

            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderService, Trippio.Data.Service.OrderService>();
            builder.Services.AddScoped<IBookingService, Trippio.Data.Service.BookingService>();
            builder.Services.AddScoped<IPaymentService, Trippio.Data.Service.PaymentService>();

            // Master Data Services
            builder.Services.AddScoped<Trippio.Core.Services.IHotelService, Trippio.Data.Services.HotelService>();
            builder.Services.AddScoped<Trippio.Core.Services.IRoomService, Trippio.Data.Services.RoomService>();
            builder.Services.AddScoped<Trippio.Core.Services.ITransportService, Trippio.Data.Services.TransportService>();
            builder.Services.AddScoped<Trippio.Core.Services.ITransportTripService, Trippio.Data.Services.TransportTripService>();
            builder.Services.AddScoped<Trippio.Core.Services.IShowService, Trippio.Data.Services.ShowService>();

            // Idempotency
            
            // Register Review Service and Repository
            builder.Services.AddScoped<Trippio.Core.Repositories.IReviewRepository, Trippio.Data.Repositories.ReviewRepository>();
            builder.Services.AddScoped<Trippio.Core.Services.IReviewService, Trippio.Data.Services.ReviewService>();
            //paymentwebhook
            builder.Services.AddScoped<IIdempotencyStore, RedisIdempotencyStore>();

            // Redis
            builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
                ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
           

            // Health Checks
            builder.Services.AddHealthChecks()
                .AddSqlServer(connectionString, name: "sql-server")
                .AddCheck("self", () => HealthCheckResult.Healthy("API is running"));

            // Controllers + System.Text.Json config (enum -> "room"/"show"/"flight")
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.CustomOperationIds(apiDesc =>
                    apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null);

                c.SwaggerDoc("TrippioAPI", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "API for Trippio",
                    Description = "API for Trippio core domain. This domain keeps track of campaigns, campaign rules, and campaign execution."
                });

                c.ParameterFilter<SwaggerNullableParameterFilter>();
                c.OperationFilter<Trippio.Api.Filters.FormFileSwaggerFilter>();

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
                        Array.Empty<string>()
                    }
                });
            });

            // AuthN
            builder.Services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(cfg =>
            {
                cfg.RequireHttpsMetadata = false;
                cfg.SaveToken = true;
                cfg.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = configuration["JwtTokenSettings:Issuer"],
                    ValidAudience = configuration["JwtTokenSettings:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtTokenSettings:Key"]))
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = configuration["Authentication:Google:ClientId"] ?? "";
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"] ?? "";
            });

            var app = builder.Build();

            // ===== Configure Static Files (Avatar, Uploads) =====
            var staticFileOptions = new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream",
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=3600"); // 1 hour cache
                }
            };
            app.UseStaticFiles(staticFileOptions);

            // ===== Enable CORS Preflight & Health Check Logging =====
            app.UseSerilogRequestLogging();

            // Add security headers middleware
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin-allow-popups");
                context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "require-corp");
                context.Response.Headers.Add("Permissions-Policy", "geolocation=(), microphone=(), camera=()");
                await next();
            });

            // CORS (sau static files, trước auth)
            app.UseCors(VietokemanPolicy);
            var enableSwagger = builder.Configuration.GetValue<bool>("Swagger__Enabled", true);
            if (enableSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/TrippioAPI/swagger.json", "Trippio API");
                    c.RoutePrefix = "swagger";
                    c.DisplayOperationId();
                    c.DisplayRequestDuration();
                    // c.InjectStylesheet("/swagger-custom.css");
                });
                app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
            }

            // Health checks
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

            // app.UseHttpsRedirection();
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
