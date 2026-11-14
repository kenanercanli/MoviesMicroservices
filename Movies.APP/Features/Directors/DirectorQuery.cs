using CORE.Domain;
using CORE.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Movies.APP.Domain;

namespace Movies.APP.Features.Directors
{
    // Request properties are created according to the data that
    // will be retrieved from APIs.
    public class DirectorQueryRequest : Record, IRequest<IQueryable<DirectorQueryResponse>>
    {
    }

    // Response properties are created according to the data to be
    // presented in API responses.
    public class DirectorQueryResponse : Record
    {
        // Copy all the non navigation properties from Director entity.
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool IsRetired { get; set; }

        // Add the new properties ending with R declaring Response
        // for custom properties or formatted string value properties.
        // FirstName + " " + LastName concatenated value.
        public string FullNameR { get; set; }

        // "Yes" or "No" according to the IsRetired value.
        public string IsRetiredR { get; set; }

        // Count of the movies of the director.
        public int MovieCountR { get; set; }

        // Concatenated names of the movies of the director.
        public string MovieNamesR { get; set; }
    }

    public class DirectorQueryHandler : DbService<Director>, IRequestHandler<DirectorQueryRequest, IQueryable<DirectorQueryResponse>>
    {
        public DirectorQueryHandler(DbContext db) : base(db)
        {
        }

        // Include the Movie entities to the query and order the query
        // by LastName first, then by FirstName.
        protected override IQueryable<Director> DbQuery()
        {
            return base.DbQuery().Include(d => d.Movies)
                .OrderBy(d => d.LastName).ThenBy(d => d.FirstName);
        }

        public Task<IQueryable<DirectorQueryResponse>> Handle(DirectorQueryRequest request, CancellationToken cancellationToken)
        {
            var query = DbQuery().Select(d => new DirectorQueryResponse
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                IsRetired = d.IsRetired,

                FullNameR = d.FirstName + " " + d.LastName,
                IsRetiredR = d.IsRetired ? "Yes" : "No",
                MovieCountR = d.Movies.Count,
                MovieNamesR = string.Join(", ", d.Movies.OrderBy(m => m.Name).Select(m => m.Name))
            });
            // d: Director entity delegate, m: Movie entity delegate.
            return Task.FromResult(query);
        }
    }
}
