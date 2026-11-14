using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

namespace Movies.APP.Features.Movies
{
    // Delete operation will be performed by the Id property inherited
    // from the Record base class.
    public class MovieRemoveRequest : Record, IRequest<CommandResponse>
    {
    }

    public class MovieRemoveHandler : DbService<Movie>, IRequestHandler<MovieRemoveRequest, CommandResponse>
    {
        public MovieRemoveHandler(DbContext db) : base(db)
        {
        }

        // Include the MovieGenre entities to the query so that
        // they can be deleted before the movie.
        protected override IQueryable<Movie> DbQuery() => base.DbQuery().Include(m => m.MovieGenres);

        public async Task<CommandResponse> Handle(MovieRemoveRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSingleAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Movie not found!");
            // Delete the related MovieGenre entities first.
            DbRemove(entity.MovieGenres);
            // Then delete the Movie entity.
            await DbRemoveAsync(entity, cancellationToken);
            return Success(entity.Id, "Movie deleted successfully.");
        }
    }
}
