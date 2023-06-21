using landerist_library.Database;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace landerist_library.Parse.Listing.MLModel
{
    public class TrainingData
    {
        public static void Create()
        {
            DataTable dataTable = ES_Listings.GetTrainingData();

        }
    }
}
