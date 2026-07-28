using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Sql;
using landerist_library.Infrastructure.Sql.Mapping;
using landerist_orels;
using landerist_orels.ES;
using System.Data;

namespace landerist_library.Infrastructure.Listings
{
    public sealed class SqlListingMediaStore
    {
        private readonly IListingMediaRepository Repository;

        public SqlListingMediaStore(IListingMediaRepository repository) => Repository = repository;

        public void Insert(Listing listing)
        {
            Repository.Insert(listing);
        }

        public void Update(Listing listing)
        {
            Delete(listing);
            Insert(listing);
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

        public SortedSet<Media> GetMedia(Listing listing)
        {
            DataTable dataTable = Repository.GetMedia(listing);

            SortedSet<Media> medias = new(new MediaComparer());
            foreach (DataRow dataRow in dataTable.Rows)
            {
                var media = GetMedia(dataRow);
                if (media != null)
                {
                    medias.Add(media);
                }
            }
            return medias;
        }

        internal Media? GetMedia(DataRow dataRow) =>
            MediaDataMapper.Map(dataRow);
    }
}
