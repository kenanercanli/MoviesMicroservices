using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

var builder = WebApplication.CreateBuilder(args);

// Add services to the IoC (Inversion of Control) container.

// Register the application's DbContext dependency injection
// by sending MoviesDb instances to the injected service class
// constructors' DbContext or MoviesDb parameter, using SQLite
// with the connection string from appsettings.json.
builder.Services.AddDbContext<DbContext, MoviesDb>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString(nameof(MoviesDb))
));
// nameof(MoviesDb) = "MoviesDb" which is the class name.

// Way 1:
// Registers MediatR handlers found in the assembly containing the MoviesDb type.
//builder.Services.AddMediatR(
//    config => config.RegisterServicesFromAssembly(typeof(MoviesDb).Assembly));
// Way 2:
// Iterates through all assemblies currently loaded in the application's app domain
// and registers all MediatR handlers (IRequestHandler implementations) found within them.
foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
{
    builder.Services.AddMediatR(
        config => config.RegisterServicesFromAssemblies(assembly));
}

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
