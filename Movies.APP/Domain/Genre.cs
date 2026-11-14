using CORE.Domain;
using System.ComponentModel.DataAnnotations;

namespace Movies.APP.Domain
{
    // Genre is a Record.
    public class Genre : Record
    {
        // Name can't be null and must contain maximum 50 characters.
        [Required, StringLength(50)]
        public string Name { get; set; }

        // Navigation property for the genres-movies many to many relationship.
        public List<MovieGenre> MovieGenres { get; set; } = new(); // navigation property

        // Since we won't update the relational movie data (MovieGenres)
        // through Genre entity, we don't need a MovieIds property here.
    }
}
