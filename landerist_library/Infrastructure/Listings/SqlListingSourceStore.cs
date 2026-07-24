using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Listings
{
    internal sealed class SqlListingSourceStore
    {
        private readonly IListingSourceRepository Repository;

        public SqlListingSourceStore(IListingSourceRepository repository) => Repository = repository;

        public void Insert(Listing listing)
        {
            Repository.Insert(listing);
        }

        public void Update(Listing listing)
        {
            if (Delete(listing))
            {
                Insert(listing);
            }
        }

        public bool Delete(Listing listing)
        {
            return Delete(listing.guid);
        }

        public bool Delete(string guid)
        {
            return Repository.Delete(guid);
        }

        public bool Delete()
        {
            return Repository.DeleteAll();
        }

        public SortedSet<Source> GetSources(Listing listing)
        {
            DataTable dataTable = Repository.GetSources(listing);

            SortedSet<Source> sources = new(new SourceComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var source = GetSource(dataRow);
                if (source != null)
                {
                    sources.Add(source);
                }
            }
            return sources;
        }

        public DataTable GetListingsWithoutSourcePages() =>
            Repository.GetListingsWithoutSourcePages();

        internal Source? GetSource(DataRow dataRow) =>
            SourceDataMapper.Map(dataRow);

    }
}
