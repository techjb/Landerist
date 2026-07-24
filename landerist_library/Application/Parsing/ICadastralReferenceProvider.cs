namespace landerist_library.Application.Parsing;

public interface ICadastralReferenceProvider
{
    string? GetCadastralReference(
        double? latitude,
        double? longitude,
        string address);
}
