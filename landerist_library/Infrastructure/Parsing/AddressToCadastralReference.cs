using landerist_library.Application.Listings;
using landerist_library.Infrastructure.Statistics;
using landerist_library.Application.Parsing;
using landerist_library.Parse.CadastralReference;
using landerist_library.Database;
using landerist_library.Infrastructure.Sql;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_library.Application.Statistics;
using Newtonsoft.Json;
using System.Data;

namespace landerist_library.Infrastructure.Parsing
{
    public sealed class AddressToCadastralReference : ICadastralReferenceProvider
    {
        private readonly AddressCadastralReference _cache;
        private readonly GlobalStatisticsRepository _statistics;
        private readonly IGoolzoomClient _goolzoom;

        public AddressToCadastralReference(IDatabase database, IGoolzoomClient goolzoom)
        {
            ArgumentNullException.ThrowIfNull(database);
            ArgumentNullException.ThrowIfNull(goolzoom);
            _cache = new AddressCadastralReference(database);
            _statistics = new GlobalStatisticsRepository(database);
            _goolzoom = goolzoom;
        }
        string? ICadastralReferenceProvider.GetCadastralReference(double? latitude, double? longitude, string address) =>
            GetCadastralReference(latitude, longitude, address);

        public string? GetCadastralReference(double? latitude, double? longitude, string address, bool firstTry = true)
        {
            if (latitude == null || longitude == null || string.IsNullOrWhiteSpace(address))
            {
                return null;
            }

            if (firstTry)
            {
                string? cachedCadastralReference = _cache.Select(address);
                if (!string.IsNullOrWhiteSpace(cachedCadastralReference))
                {
                    return cachedCadastralReference;
                }
            }

            string? cadastralReference = null;

            try
            {
                _statistics.InsertDailyCounter("AddressToCadastralReferenceRequest", 1);
                int radio = firstTry ? 50 : 100;
                var content = _goolzoom.GetAddresses(latitude.Value, longitude.Value, radio);
                if (!string.IsNullOrEmpty(content))
                {
                    var addressList = JsonConvert.DeserializeObject<AddressList>(content);
                    (bool found, cadastralReference) = GetLocalId(address, addressList);

                    if (!found && firstTry)
                    {
                        cadastralReference = GetCadastralReference(latitude, longitude, address, false);
                    }

                    if (!string.IsNullOrEmpty(cadastralReference))
                    {
                        _cache.Insert(address, cadastralReference);
                    }
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("AddressToCadastralReference GetCadastralReference", exception);
            }

            return cadastralReference;
        }

        private (bool success, string? localId) GetLocalId(string searchAddress, AddressList? addressList)
        {
            if (string.IsNullOrWhiteSpace(searchAddress) ||
                addressList?.Addresses == null ||
                addressList.Addresses.Count == 0)
            {
                return (false, null);
            }

            List<string> list = [.. addressList.Addresses
                .Where(a => !string.IsNullOrWhiteSpace(a.AddressValue))
                .Select(a => a.AddressValue!)];

            //Console.WriteLine(string.Join('\n', [.. list]));
            AddressAIFinder addressAIFinder = new(searchAddress, list);
            var result = addressAIFinder.GetAddress().GetAwaiter().GetResult();

            if (string.IsNullOrWhiteSpace(result.address) ||
                result.address.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                return (false, null);
            }

            foreach (var address in addressList.Addresses)
            {
                if (string.IsNullOrWhiteSpace(address?.AddressValue))
                {
                    continue;
                }

                if (string.Equals(address.AddressValue, result.address, StringComparison.OrdinalIgnoreCase))
                {
                    return (!string.IsNullOrWhiteSpace(address.LocalId), address.LocalId);
                }
            }

            return (false, null);
        }

        public static void UpdateCadastralReferences(IDatabase database, IListingAdministrationService listings, IGoolzoomClient goolzoom)
        {
            var items = listings.GetWithoutCadastralReferenceAndAccurateLocation();
            int total = items.Count;
            int processed = 0;
            int found = 0;
            int notFound = 0;

            if (total == 0)
            {
                Console.WriteLine("No listings to process.");
                return;
            }

            DataTable dataTableFound = new();
            dataTableFound.Columns.Add("LocalId", typeof(string));
            dataTableFound.Columns.Add("Address", typeof(string));
            dataTableFound.Columns.Add("Guid", typeof(string));

            DataTable dataTableNotFound = new();
            dataTableNotFound.Columns.Add("Address", typeof(string));

            Parallel.ForEach(items,
                new ParallelOptions() { MaxDegreeOfParallelism = 6 },
                listing =>
                {
                    var cadastralReference = new AddressToCadastralReference(database, goolzoom).GetCadastralReference(listing.latitude, listing.longitude, listing.address);
                    if (!string.IsNullOrEmpty(cadastralReference))
                    {
                        Interlocked.Increment(ref found);
                        listing.cadastralReference = cadastralReference;
                        listings.Update(listing);

                        lock (dataTableFound)
                        {
                            DataRow dataRow = dataTableFound.NewRow();
                            dataRow.ItemArray =
                            [
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

                    int currentProcessed = Interlocked.Increment(ref processed);
                    int processedPercentage = (int)((double)currentProcessed / total * 100);
                    Console.WriteLine($"Processed {currentProcessed}/{total} ({processedPercentage}%) Found: {found}  Not Found: {notFound}");
                });

            Tools.Csv.Write(dataTableFound, Configuration.AppConfig.EXPORT_DIRECTORY_LOCAL + "Found.csv", true);
            Tools.Csv.Write(dataTableNotFound, Configuration.AppConfig.EXPORT_DIRECTORY_LOCAL + "NotFound.csv", true);
        }
    }
}

