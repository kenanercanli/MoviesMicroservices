using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Movies.APP.Domain;

namespace Movies.APP.Features.Genres
{
    public class GenreUpdateRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from Genre entity.
        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(50, ErrorMessage = "{0} must be maximum {1} characters!")]
        public string Name { get; set; }
    }

    public class GenreUpdateHandler : DbService<Genre>, IRequestHandler<GenreUpdateRequest, CommandResponse>
    {
        public GenreUpdateHandler(DbContext db) : base(db)
        {
        }

        // Updates an existing genre in the Genres database table if a
        // genre with the same name other than the request record doesn't exist.
        public async Task<CommandResponse> Handle(GenreUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(g => g.Id != request.Id &&
                g.Name == request.Name.Trim(), cancellationToken))
                return Error("Genre with the same name exists!");
            var entity = await DbSingleAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Genre not found!");
            entity.Name = request.Name.Trim();
            await DbUpdateAsync(entity, cancellationToken);
            return Success(entity.Id, "Genre updated successfully.");
        }
    }
}
