using CORE.Domain;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;
using Movies.APP.Features.Directors;
using Movies.APP.Features.Genres;

namespace Movies.APP.Features.Movies
{
    // Request properties are created according to the data that
    // will be retrieved from APIs.
    public class MovieQueryRequest : Record, IRequest<IQueryable<MovieQueryResponse>>
    {
        // Optional properties for filtering the movies.
        // If a property is not sent (null), no filtering is applied for it.
        public string Name { get; set; }

        public int? DirectorId { get; set; }

        public int? GenreId { get; set; }

        public DateTime? ReleaseDateStart { get; set; }

        public DateTime? ReleaseDateEnd { get; set; }

        public decimal? TotalRevenueMin { get; set; }

        public decimal? TotalRevenueMax { get; set; }
    }

    // Response properties are created according to the data to be
    // presented in API responses.
    public class MovieQueryResponse : Record
    {
        // Copy all the non navigation properties from Movie entity.
        public string Name { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public decimal TotalRevenue { get; set; }

        public int DirectorId { get; set; }

        public List<int> GenreIds { get; set; }

        // Add the new properties ending with R declaring Response
        // for custom properties or formatted string value properties.
        public string ReleaseDateR { get; set; }

        public string TotalRevenueR { get; set; }

        // Way 1:
        public string DirectorFullNameR { get; set; }

        // Way 2:
        public DirectorQueryResponse DirectorR { get; set; }

        // Way 1:
        public string GenreNamesR { get; set; }

        // Way 2:
        public List<GenreQueryResponse> GenresR { get; set; }
    }

    public class MovieQueryHandler : DbService<Movie>, IRequestHandler<MovieQueryRequest, IQueryable<MovieQueryResponse>>
    {
        public MovieQueryHandler(DbContext db) : base(db)
        {
        }

        // Relationships:
        // Movie <-> Director.
        // Movie <-> MovieGenres <-> Genres.
        protected override IQueryable<Movie> DbQuery()
        {
            return base.DbQuery().Include(m => m.Director)
                .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
                .OrderBy(m => m.Name);
        }

        public Task<IQueryable<MovieQueryResponse>> Handle(MovieQueryRequest request, CancellationToken cancellationToken)
        {
            var entityQuery = DbQuery();

            // Filtering: each condition is added to the query only if the related
            // request property has a value, therefore an empty request returns all movies.
            if (!string.IsNullOrWhiteSpace(request.Name)) // case insensitive search in movie names
                entityQuery = entityQuery.Where(m => m.Name.ToUpper().Contains(request.Name.Trim().ToUpper()));
            if (request.DirectorId.HasValue)
                entityQuery = entityQuery.Where(m => m.DirectorId == request.DirectorId.Value);
            if (request.GenreId.HasValue) // movies having the genre in their MovieGenres
                entityQuery = entityQuery.Where(m => m.MovieGenres.Any(mg => mg.GenreId == request.GenreId.Value));
            if (request.ReleaseDateStart.HasValue)
                entityQuery = entityQuery.Where(m => m.ReleaseDate >= request.ReleaseDateStart.Value);
            if (request.ReleaseDateEnd.HasValue)
                entityQuery = entityQuery.Where(m => m.ReleaseDate <= request.ReleaseDateEnd.Value);
            if (request.TotalRevenueMin.HasValue)
                entityQuery = entityQuery.Where(m => m.TotalRevenue >= request.TotalRevenueMin.Value);
            if (request.TotalRevenueMax.HasValue)
                entityQuery = entityQuery.Where(m => m.TotalRevenue <= request.TotalRevenueMax.Value);

            var query = entityQuery.Select(m => new MovieQueryResponse
            {
                Id = m.Id,
                Name = m.Name,
                ReleaseDate = m.ReleaseDate,
                TotalRevenue = m.TotalRevenue,
                DirectorId = m.DirectorId,
                GenreIds = m.GenreIds,

                ReleaseDateR = m.ReleaseDate.HasValue ? m.ReleaseDate.Value.ToShortDateString() : string.Empty,
                TotalRevenueR = m.TotalRevenue.ToString("C2"), // C: Currency format, 2: 2 decimals.
                // Way 1:
                DirectorFullNameR = m.Director.FirstName + " " + m.Director.LastName,
                // Way 2:
                DirectorR = new DirectorQueryResponse
                {
                    Id = m.Director.Id,
                    FirstName = m.Director.FirstName,
                    LastName = m.Director.LastName,
                    IsRetired = m.Director.IsRetired,
                    FullNameR = m.Director.FirstName + " " + m.Director.LastName,
                    IsRetiredR = m.Director.IsRetired ? "Yes" : "No"
                },
                // Way 1:
                GenreNamesR = string.Join(", ", m.MovieGenres.OrderBy(mg => mg.Genre.Name).Select(mg => mg.Genre.Name)),
                // Way 2:
                GenresR = m.MovieGenres.OrderBy(mg => mg.Genre.Name).Select(mg => new GenreQueryResponse
                {
                    Id = mg.Genre.Id,
                    Name = mg.Genre.Name
                }).ToList()
            });
            // m: Movie entity delegate, mg: MovieGenre entity delegate.
            return Task.FromResult(query);
        }
    }
}
