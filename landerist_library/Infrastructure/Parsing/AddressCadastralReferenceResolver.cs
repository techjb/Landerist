using landerist_library.Application.Parsing;
using landerist_library.Parse.CadastralReference;

namespace landerist_library.Infrastructure.Parsing
{
    internal sealed class AddressCadastralReferenceResolver
    {
        private readonly ICadastralReferenceProvider Service;

        public AddressCadastralReferenceResolver(ICadastralReferenceProvider service)
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