using landerist_library.Parse.CadastralReference;

namespace landerist_library.Parse.Location.Resolvers
{
    internal sealed class AddressCadastralReferenceResolver
    {
        public string? Resolve(double? latitude, double? longitude, string? address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            return new AddressToCadastralReference().GetCadastralReference(latitude, longitude, address);
        }
    }
}
