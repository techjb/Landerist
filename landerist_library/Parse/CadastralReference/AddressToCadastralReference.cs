using landerist_library.Database;
using landerist_library.Parse.Location.Goolzoom;
using landerist_library.Statistics;
using Newtonsoft.Json;
using System.Data;

namespace landerist_library.Parse.CadastralReference
{
    public class AddressToCadastralReference
    {
        public string? GetCadastralReference(double? latitude, double? longitude, string address, bool firstTry = true)
        {
            if (latitude == null || longitude == null || address == null)
            {
                return null;
            }

            if (firstTry)
            {
                var dataTable = new AddressCadastralReference().SelectTop1(address);
                if (dataTable.Rows.Count.Equals(1))
                {
                    return dataTable.Rows[0].Field<string>("cadastralReference");
                }
            }

            bool success = false;
            string? cadastralReference = null;

            try
            {
                StatisticsSnapshot.InsertDailyCounter("AddressToCadastralReferenceRequest");
                int radio = firstTry ? 50 : 100;
                var content = new GoolzoomApi().GetAddresses((double)latitude, (double)longitude, radio);
                if (!string.IsNullOrEmpty(content))
                {
                    var addressList = JsonConvert.DeserializeObject<AddressList>(content);
                    (success, cadastralReference) = GetLocalId(address, addressList);
                    if (success && cadastralReference == null && firstTry)
                    {
                        cadastralReference = GetCadastralReference(latitude, longitude, address, false);
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("AddressToCadastralReference GetCadastralReference", exception);
            }
            if (success)
            {
                new AddressCadastralReference().Insert(address, cadastralReference);
            }
            return cadastralReference;
        }
        private (bool success, string? localId) GetLocalId(string searchAddress, AddressList? addressList)
        {
            if (string.IsNullOrEmpty(searchAddress) || addressList == null || addressList.Addresses.Count == 0)
            {
                return (false, null);
            }
            List<string> list = [.. addressList.Addresses.Select(a => a.AddressValue)];

            //Console.WriteLine(string.Join('\n', [.. list]));
            AddressAIFinder addressAIFinder = new(searchAddress, list);
            var result = addressAIFinder.GetAddress().Result;

            string? localId = null;

            if (!string.IsNullOrEmpty(result.address) && !result.address.Equals("null"))
            {
                foreach (var address in addressList.Addresses)
                {
                    if (string.IsNullOrEmpty(address?.AddressValue))
                    {
                        continue;
                    }
                    if (string.Equals(address.AddressValue, result.address, StringComparison.OrdinalIgnoreCase))
                    {
                        localId = address?.LocalId;
                        break;
                    }
                }
            }
            return (result.success, localId);
        }

        public static void UpdateCadastralReferences()
        {
            var listings = ES_Listings.GetListingsWithoutCatastralReferenceAndLocationIsAccurate();
            int total = listings.Count;
            int processed = 0;
            int found = 0;
            int notFound = 0;

            DataTable dataTableFound = new();
            dataTableFound.Columns.Add("LocalId", typeof(string));
            dataTableFound.Columns.Add("Title", typeof(string));
            dataTableFound.Columns.Add("Description", typeof(string));

            DataTable dataTableNotFound = new();
            dataTableNotFound.Columns.Add("Address", typeof(string));


            Parallel.ForEach(listings,
                new ParallelOptions() { MaxDegreeOfParallelism = 6 },
                listing =>
            {
                var cadastralReference = new AddressToCadastralReference().GetCadastralReference(listing.latitude, listing.longitude, listing.address);
                if (!string.IsNullOrEmpty(cadastralReference))
                {
                    Interlocked.Increment(ref found);
                    listing.cadastralReference = cadastralReference;
                    ES_Listings.Update(listing);


                    lock (dataTableFound)
                    {
                        DataRow dataRow = dataTableFound.NewRow();
                        dataRow.ItemArray = [
                            listing.cadastralReference,
                            listing.address,
                            listing.guid,
                        ];
                        dataTableFound.Rows.Add(dataRow);
                    }
                }
                else
                {
                    Interlocked.Increment(ref notFound);
                    lock (dataTableNotFound)
                    {
                        DataRow dataRow = dataTableNotFound.NewRow();
                        dataRow.ItemArray = [listing.address];
                        dataTableNotFound.Rows.Add(dataRow);
                    }
                }
                Interlocked.Increment(ref processed);
                int processedPercentage = (int)((double)processed / total * 100);
                Console.WriteLine($"Processed {processed}/{total} ({processedPercentage}%) Found: {found}  Not Found: {notFound}");
            });

            Tools.Csv.Write(dataTableFound, Configuration.PrivateConfig.EXPORT_DIRECTORY_LOCAL + "Found.csv", true);
            Tools.Csv.Write(dataTableNotFound, Configuration.PrivateConfig.EXPORT_DIRECTORY_LOCAL + "NotFound.csv", true);
        }
    }
}
