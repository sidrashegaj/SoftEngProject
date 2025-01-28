using BACKEND.Data; // Your DbContext
using BACKEND.Services; // Your custom services
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure the JSON serializer to handle object cycles
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true; // Optional for readability
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext for PostgreSQL
builder.Services.AddDbContext<AlbCampDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Configure CloudinarySettings
builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

// Register PhotoService
builder.Services.AddScoped<PhotoService>();

// Configure JWT Settings
builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JwtSettings"));

// Add HttpClient factory
builder.Services.AddHttpClient();

// Add GeocodingService with Mapbox configuration
builder.Services.AddScoped<GeocodingService>(provider =>
    new GeocodingService(
        provider.GetRequiredService<IHttpClientFactory>(),
        builder.Configuration["Mapbox:AccessToken"] // Ensure this key exists in appsettings.json
    )
);

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Apply migrations and create the database at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AlbCampDbContext>();
    context.Database.Migrate();
    try
    {
        DbInitializer.Initialize(context); // Call the seeder
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred seeding the database: {ex.Message}");
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

app.Run();
