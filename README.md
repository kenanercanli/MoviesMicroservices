# MoviesMicroservices

Bilkent University – CTIS 465 Microservice Development with .NET
**Project Phase 1**

**Team member:** Ahmet Kenan Ercanlı (22203536) – GitHub: [github.com/kenanercanli](https://github.com/kenanercanli)

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
└── Movies.API/     Controllers, Program.cs, appsettings.json, Templates, MoviesDB.db (SQLite)
```

Technologies: ASP.NET Core Web API (.NET 8), Entity Framework Core 8 (SQLite, code first migrations), MediatR, Swagger.

## How to Run

- **Visual Studio 2022:** open `MoviesMicroservices.sln`, set **Movies.API** as the startup project, select the **http** or **https** profile and press **F5**. Swagger opens automatically.
- **Command line:**
  ```
  cd Movies.API
  dotnet run --launch-profile http
  ```
  Then open http://localhost:5230/swagger (https profile: https://localhost:7230/swagger).

The SQLite database `Movies.API/MoviesDB.db` is already created with sample data. To reset the sample data, send a GET request to **`/api/SeedDb`** (e.g. http://localhost:5230/api/SeedDb).

## Endpoints

| Method | Route | Description |
|---|---|---|
| GET | `/api/SeedDb` | Deletes all data and inserts the sample data |
| GET | `/api/Movies` | Lists all movies (with director and genres) |
| GET | `/api/Movies/{id}` | Gets a movie by id (204 if not found) |
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

### Filtering (`POST /api/Movies/Filter`)

All filter properties are optional, only the sent ones are applied. An empty body `{}` returns all movies.

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

Sample requests for all endpoints can be found in `Movies.API/Movies.API.http`.
