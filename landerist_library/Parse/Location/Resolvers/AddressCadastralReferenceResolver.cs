using landerist_library.Parse.CadastralReference;

namespace landerist_library.Parse.Location.Resolvers
{
    internal sealed class AddressCadastralReferenceResolver
    {
        private readonly AddressToCadastralReference Service;

        public AddressCadastralReferenceResolver(AddressToCadastralReference service)
        {
            Service = service;
        }

        public string? Resolve(double? latitude, double? longitude, string? address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return null;
            }

            return Service.GetCadastralReference(latitude, longitude, address);
        }
    }
}