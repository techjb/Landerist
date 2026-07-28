using landerist_library.Database;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyAddressDataMaintenance : IAddressDataMaintenance
{
    private readonly AddressLatLng _latLng;
    private readonly AddressCadastralReference _cadastralReferences;

    public LegacyAddressDataMaintenance(IDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _latLng = new AddressLatLng(database);
        _cadastralReferences = new AddressCadastralReference(database);
    }

    public void Clean()
    {
        _latLng.Clean();
        _cadastralReferences.Clean();
    }
}
