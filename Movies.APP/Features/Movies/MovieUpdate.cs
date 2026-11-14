using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Movies.APP.Domain;

namespace Movies.APP.Features.Movies
{
    public class MovieUpdateRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from Movie entity.
        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(150, ErrorMessage = "{0} must be maximum {1} characters!")]
        public string Name { get; set; }

        public DateTime? ReleaseDate { get; set; }

        [Required(ErrorMessage = "{0} is required!")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} must be zero or a positive value!")]
        public decimal? TotalRevenue { get; set; }

        // For movies-director one to many relationship.
        [Required(ErrorMessage = "{0} is required!")]
        public int? DirectorId { get; set; }

        // For movies-genres many to many relationship.
        [Required(ErrorMessage = "At least one genre is required!")]
        public List<int> GenreIds { get; set; }
    }

    public class MovieUpdateHandler : DbService<Movie>, IRequestHandler<MovieUpdateRequest, CommandResponse>
    {
        public MovieUpdateHandler(DbContext db) : base(db)
        {
        }

        // Include the MovieGenre entities to the query. Therefore during update
        // operation, the related MovieGenre entities will be deleted first and then
        // new related MovieGenre entities will be inserted through the GenreIds property.
        protected override IQueryable<Movie> DbQuery()
        {
            return base.DbQuery().Include(m => m.MovieGenres);
        }

        public async Task<CommandResponse> Handle(MovieUpdateRequest request, CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(m => m.Id != request.Id &&
                m.Name == request.Name.Trim(), cancellationToken))
                return Error("Movie with the same name exists!");
            var entity = await DbSingleAsync(request.Id, cancellationToken);
            if (entity is null)
                return Error("Movie not found!");
            // Delete the related MovieGenre entities first.
            DbRemove(entity.MovieGenres);
            entity.Name = request.Name.Trim();
            entity.ReleaseDate = request.ReleaseDate;
            entity.TotalRevenue = request.TotalRevenue ?? 0;
            entity.DirectorId = request.DirectorId ?? 0;
            entity.GenreIds = request.GenreIds; // creates the new MovieGenres relational entities
            await DbUpdateAsync(entity, cancellationToken);
            return Success(entity.Id, "Movie updated successfully.");
        }
    }
}
