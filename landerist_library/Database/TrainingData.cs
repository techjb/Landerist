using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using landerist_library.Websites;

namespace landerist_library.Database
{
    public class TrainingData
    {
        private const string TRAINING_DATA = "[TRAINING_DATA]";


        public bool Insert(Page page)
        {
            if (!page.PageType.Equals(PageType.Listing) && !page.PageType.Equals(PageType.NotListingByParser))
            {
                return false;
            }

            string query =
                "INSERT INTO " + TRAINING_DATA + " " +
                "VALUES (@UriHash, @ResponseBodyTextHash, CONVERT(varbinary(max), @ResponseBodyZipped),  @IsListing)";
            return new DataBase().Query(query, new Dictionary<string, object?> {
                {"UriHash", page.UriHash},
                {"ResponseBodyTextHash", page.ResponseBodyTextHash},
                {"ResponseBodyZipped", page.ResponseBodyZipped },
                {"IsListing", page.PageType.Equals(PageType.Listing) },
            });
        }
    }
}
