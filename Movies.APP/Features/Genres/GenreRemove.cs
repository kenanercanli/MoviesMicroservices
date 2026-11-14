using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

namespace Movies.APP.Features.Genres
{
    // Delete operation will be performed by the Id property inherited
    // from the Record base class.
    public class GenreRemoveRequest : Record, IRequest<CommandResponse>
    {
    }

    public class GenreRemoveHandler : DbService<Genre>, IRequestHandler<GenreRemoveRequest, CommandResponse>
    {
        public GenreRemoveHandler(DbContext db) : base(db)
        {
        }

        // Include the MovieGenre entities to the query so that
        // they can be deleted before the genre.
        protected override IQueryable<Genre> DbQuery() => base.DbQuery().Include(g => g.MovieGenres);

        public async Task<CommandResponse> Handle(GenreRemoveRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSingleAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Genre not found!");
            // Delete the related MovieGenre entities first instead of relying on cascade delete.
            DbRemove(entity.MovieGenres);
            // Then delete the Genre entity.
            await DbRemoveAsync(entity, cancellationToken);
            return Success(entity.Id, "Genre deleted successfully.");
        }
    }
}
