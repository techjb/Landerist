using landerist_library.Database;
using landerist_library.Parse.Location.Goolzoom;
using landerist_library.Statistics;
using Newtonsoft.Json;
using System.Data;

namespace landerist_library.Parse.CadastralReference
{
    public class AddressToCadastralReference
    {
        public string? GetCadastalReference(double? latitude, double? longitude, string address, bool firstTry = true)
        {
            if (latitude == null || longitude == null || address == null)
            {
                return null;
            }

            string? cadastralReference = new AddressCadastralReference().Select(address);
            if (cadastralReference != null)
            {
                return cadastralReference;
            }

            try
            {
                
                StatisticsSnapshot.InsertDailyCounter("AddressToCadastralReferenceRequest");
                int radio = firstTry ? 50 : 100;
                var content = new GoolzoomApi().GetAddresses((double)latitude, (double)longitude, radio);
                if (!string.IsNullOrEmpty(content))
                {
                    var addressList = JsonConvert.DeserializeObject<AddressList>(content);
                    cadastralReference = GetLocalId(address, addressList);
                    if (cadastralReference == null && firstTry)
                    {
                        cadastralReference = GetCadastalReference(latitude, longitude, address, false);
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("AddressToCadastralReference GetCadastalReference", exception);
            }
            if (cadastralReference != null)
            {
                new AddressCadastralReference().Insert(address, cadastralReference);
                StatisticsSnapshot.InsertDailyCounter("AddressToCadastralReferenceSuccess");
            }
            return cadastralReference;
        }
        private string? GetLocalId(string searchAddress, AddressList? addressList)
        {
            if (string.IsNullOrEmpty(searchAddress) || addressList == null || addressList.Addresses.Count == 0)
            {
                return null;
            }
            AddressAIFinder addressAIFinder = new(searchAddress, [.. addressList.Addresses.Select(a => a.AddressValue)]);
            string? addressFound = addressAIFinder.GetAddress().Result;
            if (!string.IsNullOrEmpty(addressFound))
            {
                foreach (var address in addressList.Addresses)
                {
                    if (string.IsNullOrEmpty(address?.AddressValue))
                    {
                        continue;
                    }
                    if (string.Equals(address.AddressValue, addressFound, StringComparison.OrdinalIgnoreCase))
                    {
                        return address?.LocalId;
                    }
                }
            }
            return null;
        }

        public static void UpdateCadastralReferences()
        {
            var listings = ES_Listings.GetListingsWithoutCatastralReferenceAndLocationIsAccurate();
            int total = listings.Count;
            int processed = 0;
            int found = 0;
            int updated = 0;
            int notFound = 0;
            int errors = 0;

            DataTable dataTableFound = new();
            dataTableFound.Columns.Add("LocalId", typeof(string));
            dataTableFound.Columns.Add("Title", typeof(string));
            dataTableFound.Columns.Add("Description", typeof(string));

            DataTable dataTableNotFound = new();
            dataTableNotFound.Columns.Add("Address", typeof(string));


            Parallel.ForEach(listings,
                new ParallelOptions() { MaxDegreeOfParallelism = 10 },
                listing =>
            {
                Interlocked.Increment(ref processed);

                var cadastralReference = new AddressToCadastralReference().GetCadastalReference(listing.latitude, listing.longitude, listing.address);
                if (!string.IsNullOrEmpty(cadastralReference))
                {
                    Interlocked.Increment(ref found);
                    listing.cadastralReference = cadastralReference;
                    if (ES_Listings.Update(listing))
                    //if (true)
                    {
                        Interlocked.Increment(ref updated);
                    }
                    else
                    {
                        Interlocked.Increment(ref errors);
                    }


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
                Console.WriteLine($"Processed {processed}/{total}, Found: {found}, Updated: {updated}, Not Found: {notFound}");
            });

            Tools.Csv.Write(dataTableFound, Configuration.PrivateConfig.EXPORT_DIRECTORY_LOCAL + "Found.csv", true);
            Tools.Csv.Write(dataTableNotFound, Configuration.PrivateConfig.EXPORT_DIRECTORY_LOCAL + "NotFound.csv", true);
        }
    }
}
