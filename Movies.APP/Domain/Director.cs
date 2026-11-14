using CORE.Domain;
using System.ComponentModel.DataAnnotations;

namespace Movies.APP.Domain
{
    // Director is a Record.
    public class Director : Record
    {
        // FirstName can't be null and must contain maximum 50 characters.
        [Required, StringLength(50)]
        public string FirstName { get; set; }

        // LastName can't be null and must contain maximum 50 characters.
        [Required, StringLength(50)]
        public string LastName { get; set; }

        // Value Type: will have the default value false if no value is assigned.
        public bool IsRetired { get; set; }

        // Navigation property for the director-movies one to many relationship.
        // Will be an empty list of Movie if no assignment or no include to a query.
        public List<Movie> Movies { get; set; } = new(); // navigation property
    }
}
