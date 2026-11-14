using CORE.Domain;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Movies.APP.Domain
{
    // Movie is a Record.
    public class Movie : Record
    {
        // Name can't be null and must contain maximum 150 characters.
        [Required, StringLength(150)]
        public string Name { get; set; }

        // ? after the type makes the value type nullable,
        // ReleaseDate will be null if no value is assigned.
        public DateTime? ReleaseDate { get; set; }

        // Value Type: will have the default value 0 if no value is assigned.
        public decimal TotalRevenue { get; set; }

        // For movies-director one to many relationship.
        public int DirectorId { get; set; } // foreign key to the Director entity

        public Director Director { get; set; } // navigation property

        // For movies-genres many to many relationship.
        public List<MovieGenre> MovieGenres { get; set; } = new(); // navigation property

        // This property helps to easily manage the MovieGenres relational entities
        // by Movie entity's Genre Id values.
        // NotMapped attribute means no column in the Movies table will be created
        // for this property.
        [NotMapped]
        public List<int> GenreIds
        {
            // Returns the Genre Id values of the Movie entity.
            get => MovieGenres.Select(mg => mg.GenreId).ToList();
            // Sets the MovieGenres relational entities of the Movie entity
            // by the assigned Genre Id values.
            set => MovieGenres = value.Select(gId => new MovieGenre
            {
                GenreId = gId
            }).ToList();
        }
    }
}
