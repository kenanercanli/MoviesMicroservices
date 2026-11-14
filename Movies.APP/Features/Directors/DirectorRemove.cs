using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors
{
    // Delete operation will be performed by the Id property inherited
    // from the Record base class.
    public class DirectorRemoveRequest : Record, IRequest<CommandResponse>
    {
    }

    public class DirectorRemoveHandler : DbService<Director>, IRequestHandler<DirectorRemoveRequest, CommandResponse>
    {
        public DirectorRemoveHandler(DbContext db) : base(db)
        {
        }

        // Include the related Movies to the Director entity query.
        protected override IQueryable<Director> DbQuery()
        {
            return base.DbQuery().Include(d => d.Movies);
        }

        // Deletes an existing director from the Directors database table if
        // it is found by request's ID and it has no related movies.
        public async Task<CommandResponse> Handle(DirectorRemoveRequest request, CancellationToken cancellationToken)
        {
            var entity = await DbSingleAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Director not found!");
            // Check if there are any related Movies for the Director entity.
            if (entity.Movies.Any())
                return Error("Director has relational movies!");
            await DbRemoveAsync(entity, cancellationToken);
            return Success(entity.Id, "Director deleted successfully.");
        }
    }
}
