// Movies.Web: the optionally developed front-end (client) application of the project.
// This ASP.NET Core Empty project has no controllers, views or database. It only serves
// the static files (HTML, CSS and JavaScript) in the wwwroot folder to the browser.
// The page (wwwroot/index.html) uses Bootstrap 5 for the design and vanilla JavaScript
// (wwwroot/js/site.js) to consume the Movies.API endpoints through the fetch API.
// Since this application and Movies.API run on different origins (ports), CORS
// (Cross-Origin Resource Sharing) must be configured in the Program.cs of Movies.API.

// Create the web application builder with the command line arguments.
var builder = WebApplication.CreateBuilder(args);

// No services are needed to be added to the IoC (Inversion of Control) container
// since the application only serves static files.

// Build the application.
var app = builder.Build();

// Configure the HTTP request pipeline.

// Enables default file mapping for the requests to a folder: when the root URL (/)
// is requested, the request path is rewritten to /index.html (other default file
// names are index.htm, default.html and default.htm).
// UseDefaultFiles must be called before UseStaticFiles since it only rewrites the
// request path, the file is then served by the static files middleware.
app.UseDefaultFiles();

// Enables serving the static files (HTML, CSS, JavaScript, images, etc.) under the
// wwwroot folder, for example: /css/site.css is served from wwwroot/css/site.css.
app.UseStaticFiles();

// Runs the application and starts listening for incoming HTTP requests.
app.Run();
