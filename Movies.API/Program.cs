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

// ---------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) for Production Environment
// ---------------------------------------------------------------
// Registers and configures CORS services for the application.
// CORS is a security feature implemented by browsers to restrict
// cross-origin HTTP requests initiated from scripts running in the browser.
// By default, web applications are not allowed to make requests to a domain
// different from the one that served the web page.
// The configuration below adds a default CORS policy that allows requests
// from any origin, with any HTTP header, and any HTTP method.
// This is useful during development or for public APIs, but should be
// restricted in production environments to specific origins for better security.
// Usage:
// - The policy is applied globally if app.UseCors() is called without parameters
// in the middleware pipeline.
// - To restrict CORS, replace AllowAnyOrigin(), AllowAnyHeader(), and
// AllowAnyMethod() with more specific rules.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder => builder
        .AllowAnyOrigin() // Allows requests from any domain.
        .AllowAnyHeader() // Allows any HTTP headers in the request.
        .AllowAnyMethod()); // Allows any HTTP method (GET, POST, PUT, DELETE, etc.).
});

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

// ---------------------------------------------------------------
// CORS (Cross-Origin Resource Sharing) for Production Environment
// ---------------------------------------------------------------
app.UseCors();

app.Run();
