using landerist_library.Parse.CadastralReference;
using landerist_library.Infrastructure.Location.Providers.GoogleMaps;
using landerist_library.Infrastructure.Location.Providers.Goolzoom;
using landerist_library.Websites;
using landerist_orels.ES;

namespace landerist_tests
{
    internal static class LocationParserTests
    {
        public static void Run()
        {
            //var tuple1 = new landerist_library.Infrastructure.Location.Providers.GoogleMaps.GoogleMapsApi().GetLatLng("Av. Domingo Bueno, 126. O PorriÃƒÂ±o, 36.400 Pontevedra", CountryCode.ES);
            //Console.WriteLine(tuple1);

            //var tuple1 = new landerist_library.Infrastructure.Location.Providers.Goolzoom.GoolzoomApi().GetLatLng("9441515XM7094A0001FT");
            //Console.WriteLine(tuple1);

            //var tuple2 = landerist_library.Infrastructure.Location.Providers.Goolzoom.CadastralRefToLatLng.GetLatLng("9441515XM7094A");
            //Console.WriteLine(tuple2);

            //Console.WriteLine(landerist_library.Tools.Validate.CadastralReference("3979515DD7737H0002LX"));
            //landerist_library.Tools.Validate.RemoveInvalidCatastralReferences();

            //string address = "Fuengirola, Torreblanca del Sol, MÃƒÂ¡laga, EspaÃƒÂ±a, 29640";
            //string address = "Calle Alondra 8, 28232, las rozas de madrid";
            //var latLNg = new GoogleMapsApi().GetLatLng(address, CountryCode.ES);
            // var cadastralReference = new GoolzoomApi().GetAddresses(latLNg.Value.Latitude, latLNg.Value.Longitude, 10);
            //Console.WriteLine(cadastralReference);

            //Console.WriteLine(d.latLng.ToString() + " " +  d.isAccurate);
            //GoogleMapsApi.UpdateListingsLocationIsAccurate();
            //CadastralRefToLatLng.UpdateLocationFromCadastralRef();
            //Console.WriteLine(new CadastralRefToLatLng().GetLatLng("F239324UK8141N0001HP"));
            //Console.WriteLine(new GoolzoomApi().GetAddrees("7979409YJ1677N0005BE"));
            //GoolzoomApi.UpdateAddressFromCadastralRef();
            //new landerist_library.GetLatLng.Location.GoogleMaps.GoogleMapsApi().GetLatLng(listing.address);
            //new GoolzoomApi().GetAddresses(40.4243178, -3.7021782, 50);
        }
    }
}

