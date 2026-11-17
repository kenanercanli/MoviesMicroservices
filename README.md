# MoviesMicroservices

Bilkent University – CTIS 465 Microservice Development with .NET
**Project Phase 1**

**Team member:** Ahmet Kenan Ercanlı (22203536) – GitHub: [github.com/kenanercanli](https://github.com/kenanercanli)

## Phase 1 Scope

- CRUD operations for **Movies**, **Directors** and **Genres**. The **MovieGenre** relational entity is managed through the `genreIds` property of a movie (create, update and delete of a movie create, replace and delete its movie genres).
- No authentication or authorization in Phase 1. JWT authentication, role based authorization and the Ocelot API Gateway are planned for the final phase.
- Bonus: filtered retrieval of movies with `POST /api/Movies/Filter`.
- Optional client application: **Movies.Web** (see [How to Run](#how-to-run)).

## Domain

A movies microservice that manages movies, their directors and their genres.

| Entity | Properties |
|---|---|
| **Movie** | Id, Name (required, max 150), ReleaseDate (optional), TotalRevenue, DirectorId |
| **Director** | Id, FirstName (required, max 50), LastName (required, max 50), IsRetired |
| **Genre** | Id, Name (required, max 50) |
| **MovieGenre** | Id, MovieId, GenreId (relational entity) |

Relationships:
- **Director 1 – many Movie** (a director has many movies, a movie has one director)
- **Movie many – many Genre** through the **MovieGenre** relational entity

## Project Structure

```
MoviesMicroservices.sln
├── CORE/           Base classes: Record, CommandResponse, Service, DbService
├── Movies.APP/     Domain (entities, MoviesDb, MoviesDbFactory), Features (MediatR requests,
│                   responses and handlers for Movies, Directors and Genres), Migrations
├── Movies.API/     Controllers, Program.cs (with CORS), appsettings.json, Templates,
│                   Movies.API.http, MoviesDB.db (SQLite)
└── Movies.Web/     Optional client: ASP.NET Core Empty project serving the static files in
                    wwwroot (index.html, css/site.css, js/site.js)
```

Technologies:
- **API:** ASP.NET Core Web API (.NET 8), Entity Framework Core 8 (SQLite, code first migrations), MediatR, Swagger, CORS (default policy allowing any origin, header and method).
- **Client:** HTML, Bootstrap 5.3.3 (loaded from the jsDelivr CDN) and vanilla JavaScript (`fetch` API), served by an ASP.NET Core (.NET 8) Empty project.

## Prerequisites

- **Visual Studio 2022** with the **ASP.NET and web development** workload, or the **.NET 8 SDK** for the command line.
- Internet access for the client, since Bootstrap is loaded from a CDN.

## How to Run

| Project | Launch profile | URL |
|---|---|---|
| Movies.API | `http` (first, default) | http://localhost:5230 (Swagger: http://localhost:5230/swagger) |
| Movies.API | `https` | https://localhost:7230 (and http://localhost:5230) |
| Movies.Web | `http` | http://localhost:5240 |
| Movies.Web | `https` | https://localhost:7240 (and http://localhost:5240) |

### 1. API only

- **Visual Studio 2022:** open `MoviesMicroservices.sln`, set **Movies.API** as the startup project, select the **http** profile and press **F5**. Swagger opens automatically.
- **Command line** (in the solution folder):
  ```
  dotnet run --project Movies.API --launch-profile http
  ```
  Then open http://localhost:5230/swagger.

### 2. API and client together

- **Visual Studio 2022:** right-click the solution, click **Configure Startup Projects**, select **Multiple startup projects** and set the **Action** of **Movies.API** and **Movies.Web** to **Start** (http profiles). Press **F5**: Swagger and the client (http://localhost:5240) open.
- **Command line:** run the two projects in two terminals (in the solution folder):
  ```
  dotnet run --project Movies.API --launch-profile http
  ```
  ```
  dotnet run --project Movies.Web --launch-profile http
  ```
  Then open http://localhost:5240.

### 3. Client without Movies.Web

While the API is running, `Movies.Web/wwwroot/index.html` can also be opened directly in a browser.

### The client (Movies.Web)

The client calls the API at `http://localhost:5230/api` and, if it does not answer, at `https://localhost:7230/api` (the browser must trust the ASP.NET Core development certificate for https). The badge in the navigation bar shows the connected API address. Movies.API allows these cross-origin requests with its CORS configuration in `Program.cs`.

- **Movies**, **Directors** and **Genres** tabs, each with a list and **New**, **Edit** and **Delete** operations (create and edit in modal forms, delete after confirmation). A movie's director is selected from a list and its genres with checkboxes.
- **Filter Movies** panel (name, director, genre, release date range, total revenue range) that uses `POST /api/Movies/Filter`.
- **Reset Sample Data** button (`GET /api/SeedDb`, after confirmation) and a **Swagger** link.

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/SeedDb` | Deletes all data and inserts the sample data |
| GET | `/api/Movies` | Lists all movies (with director and genres) |
| GET | `/api/Movies/{id}` | Gets a movie by id |
| POST | `/api/Movies` | Creates a movie |
| PUT | `/api/Movies` | Updates a movie (including its genres) |
| DELETE | `/api/Movies/{id}` | Deletes a movie with its movie genres |
| POST | `/api/Movies/Filter` | Lists the movies filtered by the sent values |
| GET | `/api/Directors` | Lists all directors |
| GET | `/api/Directors/{id}` | Gets a director by id |
| POST | `/api/Directors` | Creates a director |
| PUT | `/api/Directors` | Updates a director |
| DELETE | `/api/Directors/{id}` | Deletes a director (not allowed if the director has movies) |
| GET | `/api/Genres` | Lists all genres |
| GET | `/api/Genres/{id}` | Gets a genre by id |
| POST | `/api/Genres` | Creates a genre |
| PUT | `/api/Genres` | Updates a genre |
| DELETE | `/api/Genres/{id}` | Deletes a genre with its movie genres |

Example movie create request body (`POST /api/Movies`):

```json
{
  "name": "Oppenheimer",
  "releaseDate": "2023-07-21",
  "totalRevenue": 975594977,
  "directorId": 1,
  "genreIds": [ 4 ]
}
```

For `PUT` requests, also send the `id` of the record to update.

Movie create and update rules:
- `directorId` and every value in `genreIds` must be ids of existing records (see `GET /api/Directors` and `GET /api/Genres`), otherwise the response is 400 with `"Director not found!"` or `"Genre not found!"`.
- At least one genre is required. Repeated genre ids are ignored (e.g. `[1, 1, 2]` is saved as `[1, 2]`).
- Swagger prefills the request body with example values (`"string"`, `0`, `[0]`); replace them with real values, otherwise the response is an error such as `"Director not found!"`.

## Responses

Create, update and delete operations return a `CommandResponse`:

```json
{
  "isSuccessful": true,
  "message": "Movie created successfully.",
  "id": 7
}
```

| Status | When | Body |
|---|---|---|
| 200 OK | Successful get, list, filter, create, update or delete | Record(s) or `CommandResponse` (`id` of the created, updated or deleted record) |
| 204 No Content | Get by id finds no record; list or filter returns no records | Empty |
| 400 Bad Request | Business rule errors: duplicate name, record not found on update or delete, director has movies (`"Director has relational movies!"`), director or genre not found on movie create or update | `CommandResponse` with `isSuccessful: false` and the error `message` (`id` is 0) |
| 400 Bad Request | Model validation errors (data annotations, checked automatically by `[ApiController]`), e.g. missing name or empty `genreIds` | ProblemDetails with an `errors` object, e.g. `"GenreIds": ["At least one genre is required!"]` |
| 500 Internal Server Error | Unexpected exceptions in the entity controllers | `CommandResponse` with a message such as `"An exception occured during MoviesPost."` |

`GET /api/SeedDb` returns 200 with the plain text `Database seed successful.`

## Filtering (`POST /api/Movies/Filter`)

All filter properties are optional, only the sent ones are applied. An empty body `{}` returns all movies (204 if no movie matches).

```json
{
  "name": "the",
  "directorId": 1,
  "genreId": 4,
  "releaseDateStart": "2000-01-01",
  "releaseDateEnd": "2015-12-31",
  "totalRevenueMin": 500000000,
  "totalRevenueMax": 1100000000
}
```

- `name`: movies whose names contain the value (case insensitive)
- `directorId`: movies of the director
- `genreId`: movies having the genre
- `releaseDateStart` / `releaseDateEnd`: release date range
- `totalRevenueMin` / `totalRevenueMax`: total revenue range

Notes:
- Swagger prefills every property with placeholder values (`"string"`, `0` and the current date), which filter out all movies (204). Delete the properties you don't use, or send `{}`.
- The inherited `id` property is ignored by the filter.

## Database

- The SQLite database `Movies.API/MoviesDB.db` is included and already contains sample data.
- To reset the sample data, send `GET /api/SeedDb` (e.g. http://localhost:5230/api/SeedDb) or click **Reset Sample Data** in the client. The ids start from 1 again.
- To recreate the database from the code first migration (`v1` in `Movies.APP/Migrations`), stop the API, delete `Movies.API/MoviesDB.db` (and `MoviesDB.db-shm` / `MoviesDB.db-wal` if they exist), then:
  - **Visual Studio:** set **Movies.API** as the startup project, open **Tools > NuGet Package Manager > Package Manager Console**, set **Movies.APP** as the **Default project** and run:
    ```
    update-database
    ```
  - **Command line** (in the solution folder, with `dotnet tool install -g dotnet-ef`):
    ```
    dotnet ef database update -p Movies.APP -s Movies.API
    ```
  The database is created in the Movies.API project. Then run the API and send `GET /api/SeedDb` to insert the sample data. (After changing the entities, a new migration is added first with `add-migration v2` in the Package Manager Console, or `dotnet ef migrations add v2 -p Movies.APP -s Movies.API`.)

## Sample Requests

`Movies.API/Movies.API.http` contains sample requests for all endpoints (Visual Studio can send them). Start the API with the **http** profile and send `GET /api/SeedDb` first, because the sample ids assume freshly seeded data.
