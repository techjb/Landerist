using landerist_library.Parse.CadastralReference;
using landerist_library.Parse.Location.Providers.GoogleMaps;
using landerist_library.Parse.Location.Providers.Goolzoom;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_tests
{
    internal static class LocationParserTests
    {
        public static void Run()
        {
            //var tuple1 = new landerist_library.Parse.Location.Providers.GoogleMaps.GoogleMapsApi().GetLatLng("Av. Domingo Bueno, 126. O Porriño, 36.400 Pontevedra", CountryCode.ES);
            //Console.WriteLine(tuple1);

            //var tuple1 = new landerist_library.Parse.Location.Providers.Goolzoom.GoolzoomApi().GetLatLng("9441515XM7094A0001FT");
            //Console.WriteLine(tuple1);

            //var tuple2 = landerist_library.Parse.Location.Providers.Goolzoom.CadastralRefToLatLng.GetLatLng("9441515XM7094A");
            //Console.WriteLine(tuple2);

            //Console.WriteLine(landerist_library.Tools.Validate.CadastralReference("3979515DD7737H0002LX"));
            //landerist_library.Tools.Validate.RemoveInvalidCatastralReferences();

            //string address = "Fuengirola, Torreblanca del Sol, Málaga, España, 29640";
            //string address = "Calle Alondra 8, 28232, las rozas de madrid";
            //var latLNg = new GoogleMapsApi().GetLatLng(address, CountryCode.ES);
            // var cadastralReference = new GoolzoomApi().GetAddresses(latLNg.Value.Latitude, latLNg.Value.Longitude, 10);
            //var cadastralReference = new AddressToCadastralReference().GetCadastralReference(latLNg.Value.Latitude, latLNg.Value.Longitude, address);
            //Console.WriteLine(cadastralReference);

            //Console.WriteLine(d.latLng.ToString() + " " +  d.isAccurate);
            //GoogleMapsApi.UpdateListingsLocationIsAccurate();
            //CadastralRefToLatLng.UpdateLocationFromCadastralRef();
            //Console.WriteLine(new CadastralRefToLatLng().GetLatLng("F239324UK8141N0001HP"));
            //Console.WriteLine(new GoolzoomApi().GetAddrees("7979409YJ1677N0005BE"));
            //GoolzoomApi.UpdateAddressFromCadastralRef();
            //AddressToCadastralReference.UpdateCadastralReferences();
            //var listing = ES_Listings.GetListing("0074C7FF345F923A06992C15431EA2630A114713CC96D6DDA8DE35372286902A");
            //new landerist_library.GetLatLng.Location.GoogleMaps.GoogleMapsApi().GetLatLng(listing.address);
            //new GoolzoomApi().GetAddresses(40.4243178, -3.7021782, 50);
        }
    }
}

