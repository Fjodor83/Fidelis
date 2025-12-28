using Fidelity.Server.Data;
using Fidelity.Server.Services;
using Fidelity.Server.Middleware;
using Fidelity.Server.Repositories;
using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Clienti.Commands.RegistraCliente;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/fidelity-.txt", rollingInterval: RollingInterval.Day)
    .CreateBootstrapLogger();

try
{

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File("logs/fidelity-.txt",
            rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"));

    if (builder.Environment.IsDevelopment())
    {
        builder.Configuration.AddUserSecrets<Program>();
    }
    builder.Configuration.AddEnvironmentVariables();

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    // OLD DbContext (will be phased out)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    // NEW Clean Architecture DbContext
    builder.Services.AddDbContext<Fidelity.Infrastructure.Persistence.ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Services.AddScoped<IApplicationDbContext>(provider =>
        provider.GetRequiredService<Fidelity.Infrastructure.Persistence.ApplicationDbContext>());

    builder.Services.AddControllersWithViews();
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>();
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();
    builder.Services.AddRazorPages();

    // Clean Architecture Services
    builder.Services.AddMediatR(cfg => {
        cfg.RegisterServicesFromAssembly(typeof(RegistraClienteCommand).Assembly);
    });
    builder.Services.AddValidatorsFromAssembly(typeof(RegistraClienteCommandValidator).Assembly);

    // Infrastructure Services
    builder.Services.AddScoped<Fidelity.Application.Common.Interfaces.IJwtService, Fidelity.Infrastructure.Services.JwtService>();

    // Services Registration (Legacy + V2)
    // Email Services - Registrazione corretta per evitare errori di DI
    builder.Services.AddScoped<Fidelity.Server.Services.EmailService>(); // Registra la classe concreta
    builder.Services.AddScoped<Fidelity.Server.Services.IEmailService>(provider =>
        provider.GetRequiredService<Fidelity.Server.Services.EmailService>()); // Interfaccia Server
    builder.Services.AddScoped<Fidelity.Application.Common.Interfaces.IEmailService, Fidelity.Server.Services.EmailServiceAdapter>(); // Adapter per Application

    builder.Services.AddScoped<ICardGeneratorService, CardGeneratorService>();
    builder.Services.AddAutoMapper(typeof(Program));
    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<ITransazioneService, TransazioneService>();
    builder.Services.AddScoped<ICouponService, CouponService>();
    builder.Services.AddScoped<IClienteService, ClienteService>();
    builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
    builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    builder.Services.AddScoped<IClienteRepository, ClienteRepository>();

    // Rate Limiting Configuration
    builder.Services.AddSingleton(new RateLimitOptions
    {
        RequestLimit = builder.Configuration.GetValue<int>("RateLimiting:RequestLimit", 100),
        Window = TimeSpan.FromMinutes(builder.Configuration.GetValue<int>("RateLimiting:WindowMinutes", 1))
    });

    // Request Size Limits
    builder.Services.Configure<IISServerOptions>(options =>
    {
        options.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    });

    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10 MB
    });

    // CORS Configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowBlazorClient", policy =>
        {
            policy.WithOrigins(
                "http://localhost:5184",  // Client dev server
                "https://localhost:7184", // Client dev server HTTPS
                "http://localhost:5085",  // Server itself
                "https://localhost:7085"  // Server HTTPS
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
        });
    });

    // Authentication Configuration
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
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
            };
        });

    // Swagger/OpenAPI Configuration
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    // ===== SEED DATABASE ALL'AVVIO =====
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Applica migrations automaticamente
            await context.Database.MigrateAsync();

            // Seed data
            await DatabaseSeeder.SeedAsync(context);

            Console.WriteLine("✓ Database initialized successfully!");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Suns Fidelity API V1");
            c.RoutePrefix = "swagger";
        });
    }
    else
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseExceptionHandler();

    app.UseMiddleware<SecurityHeadersMiddleware>();
    app.UseMiddleware<RateLimitingMiddleware>();

    app.UseHttpsRedirection();

    // CORS deve essere prima di Authorization
    app.UseCors("AllowBlazorClient");

    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapRazorPages();
    app.MapControllers();
    app.MapHealthChecks("/health");
    // Fallback for Blazor WASM
    app.MapFallbackToFile("index.html");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}