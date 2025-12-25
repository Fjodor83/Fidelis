using Fidelity.Server.Data;
using Fidelity.Server.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Services Registration
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICardGeneratorService, CardGeneratorService>();
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ITransazioneService, TransazioneService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

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
// Fallback for Blazor WASM
app.MapFallbackToFile("index.html");

app.Run();