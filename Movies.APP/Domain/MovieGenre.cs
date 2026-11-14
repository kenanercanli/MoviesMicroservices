using CORE.Domain;

namespace Movies.APP.Domain
{
    // MovieGenre is a Record.
    // For movies-genres many to many relationship.
    public class MovieGenre : Record
    {
        public int MovieId { get; set; } // foreign key to the Movie entity

        public Movie Movie { get; set; } // navigation property

        public int GenreId { get; set; } // foreign key to the Genre entity

        public Genre Genre { get; set; } // navigation property
    }
}
