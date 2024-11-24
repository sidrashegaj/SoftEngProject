using BACKEND.Data; // Your DbContext
using BACKEND.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext for PostgreSQL
builder.Services.AddDbContext<AlbCampDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")); // For SQL Server
});

builder.Services.Configure<JWTSettings>(builder.Configuration.GetSection("JwtSettings"));


var app = builder.Build();

// Apply migrations and create the database at startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AlbCampDbContext>();
    // Automatically apply migrations to create/update the database schema
    context.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
