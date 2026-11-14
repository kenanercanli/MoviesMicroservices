using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors
{
    public class DirectorUpdateRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from Director entity.
        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(50, ErrorMessage = "{0} must be maximum {1} characters!")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(50, ErrorMessage = "{0} must be maximum {1} characters!")]
        public string LastName { get; set; }

        public bool IsRetired { get; set; }
    }

    public class DirectorUpdateHandler : DbService<Director>, IRequestHandler<DirectorUpdateRequest, CommandResponse>
    {
        public DirectorUpdateHandler(DbContext db) : base(db)
        {
        }

        // Updates an existing director in the Directors database table if a
        // director with the same first name and last name other than the
        // request record doesn't exist.
        public async Task<CommandResponse> Handle(DirectorUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(d => d.Id != request.Id &&
                d.FirstName == request.FirstName.Trim() &&
                d.LastName == request.LastName.Trim(), cancellationToken))
                return Error("Director with the same first name and last name exists!");
            var entity = await DbSingleAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Director not found!");
            entity.FirstName = request.FirstName.Trim();
            entity.LastName = request.LastName.Trim();
            entity.IsRetired = request.IsRetired;
            await DbUpdateAsync(entity, cancellationToken);
            return Success(entity.Id, "Director updated successfully.");
        }
    }
}
