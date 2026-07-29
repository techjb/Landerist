using landerist_library.Database;
using landerist_library.Infrastructure.Tasks;

namespace landerist_library.Infrastructure.Administration;

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
