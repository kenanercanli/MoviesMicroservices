using CORE.Domain;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

namespace Movies.APP.Features.Genres
{
    // Request properties are created according to the data that
    // will be retrieved from APIs.
    public class GenreQueryRequest : Record, IRequest<IQueryable<GenreQueryResponse>>
    {
    }

    // Response properties are created according to the data to be
    // presented in API responses.
    public class GenreQueryResponse : Record
    {
        // Copy all the non navigation properties from Genre entity.
        public string Name { get; set; }

        // Add the new properties ending with R declaring Response
        // for custom properties or formatted string value properties.
        // Count of the movies of the genre.
        public int MovieCountR { get; set; }

        // Concatenated names of the movies of the genre.
        public string MovieNamesR { get; set; }
    }

    public class GenreQueryHandler : DbService<Genre>, IRequestHandler<GenreQueryRequest, IQueryable<GenreQueryResponse>>
    {
        public GenreQueryHandler(DbContext db) : base(db)
        {
        }

        // Include the MovieGenre and Movie entities to the query
        // and order the query by Genre entity Name property.
        protected override IQueryable<Genre> DbQuery()
        {
            return base.DbQuery().Include(g => g.MovieGenres).ThenInclude(mg => mg.Movie)
                .OrderBy(g => g.Name);
        }

        public Task<IQueryable<GenreQueryResponse>> Handle(GenreQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbQuery().Select(g => new GenreQueryResponse
            {
                Id = g.Id,
                Name = g.Name,

                MovieCountR = g.MovieGenres.Count,
                MovieNamesR = string.Join(", ", g.MovieGenres.OrderBy(mg => mg.Movie.Name).Select(mg => mg.Movie.Name))
            });
            // g: Genre entity delegate, mg: MovieGenre entity delegate.
            return Task.FromResult(query);
        }
    }
}
