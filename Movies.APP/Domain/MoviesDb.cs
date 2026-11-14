using Microsoft.EntityFrameworkCore;

namespace Movies.APP.Domain
{
    // Inherits from the Entity Framework's DbContext class for
    // database configurations and operations.
    public class MoviesDb : DbContext
    {
        // Represents the Movies table in the database.
        public DbSet<Movie> Movies { get; set; }

        // Represents the Directors table in the database.
        public DbSet<Director> Directors { get; set; }

        // Represents the Genres table in the database.
        public DbSet<Genre> Genres { get; set; }

        // Represents the MovieGenres table in the database.
        public DbSet<MovieGenre> MovieGenres { get; set; }

        // The options parameter is used to configure the database
        // connection with such as the connection string.
        public MoviesDb(DbContextOptions options) : base(options)
        {
        }
    }
}
