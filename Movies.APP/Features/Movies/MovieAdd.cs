using CORE.Domain;
using CORE.Models;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Movies.APP.Domain;

namespace Movies.APP.Features.Movies
{
    public class MovieAddRequest : Record, IRequest<CommandResponse>
    {
        // Copy all the non navigation properties from Movie entity.
        [Required(ErrorMessage = "{0} is required!")]
        [StringLength(150, ErrorMessage = "{0} must be maximum {1} characters!")]
        public string Name { get; set; }

        public DateTime? ReleaseDate { get; set; }

        // The type changed from decimal to decimal? to be able to use [Required] attribute.
        [Required(ErrorMessage = "{0} is required!")]
        [Range(0, double.MaxValue, ErrorMessage = "{0} must be zero or a positive value!")]
        public decimal? TotalRevenue { get; set; }

        // For movies-director one to many relationship.
        // The type changed from int to int? to be able to use [Required] attribute.
        [Required(ErrorMessage = "{0} is required!")]
        public int? DirectorId { get; set; }

        // For movies-genres many to many relationship.
        [Required(ErrorMessage = "At least one genre is required!")]
        public List<int> GenreIds { get; set; }
    }

    public class MovieAddHandler : DbService<Movie>, IRequestHandler<MovieAddRequest, CommandResponse>
    {
        public MovieAddHandler(DbContext db) : base(db)
        {
        }

        // Inserts a new movie to the Movies database table with its MovieGenres
        // if a movie with the same name doesn't exist.
        public async Task<CommandResponse> Handle(MovieAddRequest request, CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(m => m.Name == request.Name.Trim(), cancellationToken))
                return Error("Movie with the same name exists!");
            var entity = new Movie
            {
                Name = request.Name.Trim(),
                ReleaseDate = request.ReleaseDate,
                TotalRevenue = request.TotalRevenue ?? 0,
                DirectorId = request.DirectorId ?? 0,
                GenreIds = request.GenreIds // creates the MovieGenres relational entities
            };
            await DbAddAsync(entity, cancellationToken);
            return Success(entity.Id, "Movie created successfully.");
        }
    }
}
