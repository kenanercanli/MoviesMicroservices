using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors
{
    public class DirectorAddRequest : Record, IRequest<CommandResponse>
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

    public class DirectorAddHandler : DbService<Director>, IRequestHandler<DirectorAddRequest, CommandResponse>
    {
        public DirectorAddHandler(DbContext db) : base(db)
        {
        }

        // Inserts a new director to the Directors database table if a
        // director with the same first name and last name doesn't exist.
        public async Task<CommandResponse> Handle(DirectorAddRequest request, CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(d => d.FirstName == request.FirstName.Trim() &&
                d.LastName == request.LastName.Trim(), cancellationToken))
                return Error("Director with the same first name and last name exists!");
            var entity = new Director
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                IsRetired = request.IsRetired
            };
            await DbAddAsync(entity, cancellationToken);
            return Success(entity.Id, "Director created successfully.");
        }
    }
}
