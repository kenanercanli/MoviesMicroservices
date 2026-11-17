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
        // Required checks that GenreIds is sent (not null), MinLength checks that
        // the sent collection has at least 1 element (an empty array [] is not valid).
        [Required(ErrorMessage = "At least one genre is required!")]
        [MinLength(1, ErrorMessage = "At least one genre is required!")]
        public List<int> GenreIds { get; set; }
    }

    public class MovieAddHandler : DbService<Movie>, IRequestHandler<MovieAddRequest, CommandResponse>
    {
        // The DbContext field in the DbService base class is private, therefore we keep
        // the injected DbContext also in this handler's own field to be able to query
        // the related Directors and Genres tables.
        private readonly DbContext _db;

        public MovieAddHandler(DbContext db) : base(db)
        {
            _db = db;
        }

        // Inserts a new movie to the Movies database table with its MovieGenres
        // if a movie with the same name doesn't exist and the sent director and genres exist.
        public async Task<CommandResponse> Handle(MovieAddRequest request, CancellationToken cancellationToken)
        {
            if (await DbQuery().AnyAsync(m => m.Name == request.Name.Trim(), cancellationToken))
                return Error("Movie with the same name exists!");

            // If the director with the sent DirectorId doesn't exist in the Directors table,
            // return an error response instead of getting a foreign key exception while saving.
            if (!await _db.Set<Director>().AnyAsync(d => d.Id == request.DirectorId, cancellationToken))
                return Error("Director not found!");

            // Distinct removes the repeating genre ids, for example [1, 1, 2] becomes [1, 2],
            // therefore the same MovieGenre relational entity won't be created more than once.
            var genreIds = request.GenreIds.Distinct().ToList();

            // If the count of the genres found in the Genres table by the sent genre ids is not
            // equal to the count of the genre ids, at least one of the genres doesn't exist.
            if (await _db.Set<Genre>().CountAsync(g => genreIds.Contains(g.Id), cancellationToken) != genreIds.Count)
                return Error("Genre not found!");

            var entity = new Movie
            {
                Name = request.Name.Trim(),
                ReleaseDate = request.ReleaseDate,
                TotalRevenue = request.TotalRevenue ?? 0,
                DirectorId = request.DirectorId ?? 0,
                GenreIds = genreIds // creates the MovieGenres relational entities
            };
            await DbAddAsync(entity, cancellationToken);
            return Success(entity.Id, "Movie created successfully.");
        }
    }
}
