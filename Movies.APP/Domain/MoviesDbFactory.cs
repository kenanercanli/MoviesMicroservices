using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Movies.APP.Domain
{
    // Provides a factory for creating MoviesDb instances at design time.
    // This is used by Entity Framework (EF) tools such as
    // migrations to construct the database context when the
    // application is not running.
    // This class may need to be created if there are any exceptions
    // during Scaffolding.
    public class MoviesDbFactory : IDesignTimeDbContextFactory<MoviesDb>
    {
        // The connection string in the configuration file
        // (appsettings.json).
        const string CONNECTIONSTRING = "data source=MoviesDB.db";

        // Creates a new instance of the MoviesDb type using the connection
        // string. This method is called by EF tooling at design time.
        public MoviesDb CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MoviesDb>();
            optionsBuilder.UseSqlite(CONNECTIONSTRING);
            return new MoviesDb(optionsBuilder.Options);
        }

        // Seeds the database tables with initial data (Optional).
        public void SeedDb()
        {
            // Create a new instance of the MoviesDb type:
            var db = CreateDbContext(null);

            // Delete existing data:
            var movieGenres = db.MovieGenres.ToList();
            db.MovieGenres.RemoveRange(movieGenres);
            var movies = db.Movies.ToList();
            db.Movies.RemoveRange(movies);
            var genres = db.Genres.ToList();
            db.Genres.RemoveRange(genres);
            var directors = db.Directors.ToList();
            db.Directors.RemoveRange(directors);

            // Reset identity columns (for SQLite):
            // IDs will start from 1
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='MovieGenres'");
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='Movies'");
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='Genres'");
            db.Database.ExecuteSqlRaw("UPDATE SQLITE_SEQUENCE " +
                "SET SEQ=0 WHERE NAME='Directors'");

            // Insert new data:
            // Genres:
            db.Genres.Add(new Genre { Name = "Action" });
            db.Genres.Add(new Genre { Name = "Comedy" });
            db.Genres.Add(new Genre { Name = "Crime" });
            db.Genres.Add(new Genre { Name = "Drama" });
            db.Genres.Add(new Genre { Name = "Science Fiction" });
            db.SaveChanges(); // commit changes to the database

            // Directors with Movies:
            db.Directors.Add(new Director
            {
                FirstName = "Christopher",
                LastName = "Nolan",
                IsRetired = false,
                Movies = new List<Movie>
                {
                    new Movie
                    {
                        Name = "Inception",
                        ReleaseDate = new DateTime(2010, 7, 16),
                        TotalRevenue = 839030630.00m,
                        MovieGenres = new List<MovieGenre>
                        {
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Action").Id },
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Science Fiction").Id }
                        }
                    },
                    new Movie
                    {
                        Name = "Interstellar",
                        ReleaseDate = new DateTime(2014, 11, 7),
                        TotalRevenue = 773867216.00m,
                        MovieGenres = new List<MovieGenre>
                        {
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Drama").Id },
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Science Fiction").Id }
                        }
                    },
                    new Movie
                    {
                        Name = "The Dark Knight",
                        ReleaseDate = new DateTime(2008, 7, 18),
                        TotalRevenue = 1006234167.00m,
                        MovieGenres = new List<MovieGenre>
                        {
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Action").Id },
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Crime").Id },
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Drama").Id }
                        }
                    }
                }
            });
            db.Directors.Add(new Director
            {
                FirstName = "Quentin",
                LastName = "Tarantino",
                IsRetired = false,
                Movies = new List<Movie>
                {
                    new Movie
                    {
                        Name = "Pulp Fiction",
                        ReleaseDate = new DateTime(1994, 10, 14),
                        TotalRevenue = 213928762.00m,
                        MovieGenres = new List<MovieGenre>
                        {
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Crime").Id },
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Drama").Id }
                        }
                    },
                    new Movie
                    {
                        Name = "Django Unchained",
                        ReleaseDate = new DateTime(2012, 12, 25),
                        TotalRevenue = 426074373.00m,
                        MovieGenres = new List<MovieGenre>
                        {
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Action").Id },
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Drama").Id }
                        }
                    }
                }
            });
            db.Directors.Add(new Director
            {
                FirstName = "Peter",
                LastName = "Weir",
                IsRetired = true,
                Movies = new List<Movie>
                {
                    new Movie
                    {
                        Name = "The Truman Show",
                        ReleaseDate = new DateTime(1998, 6, 5),
                        TotalRevenue = 264118201.00m,
                        MovieGenres = new List<MovieGenre>
                        {
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Comedy").Id },
                            new MovieGenre { GenreId = db.Genres.Single(
                                g => g.Name == "Drama").Id }
                        }
                    }
                }
            });
            db.SaveChanges();
        }
    }
}
