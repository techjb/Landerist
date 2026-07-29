using landerist_library.Infrastructure.Ai.StructuredOutputs;
using landerist_domain.Parsing.StructuredOutputs;
using landerist_library.Parse.ListingParser.StructuredOutputs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace landerist_unit_tests;

public sealed class StructuredOutputSerializationCompatibilityTests
{
    [Fact]
    public void Newtonsoft_RoundTripUsesSpanishDataMemberNames()
    {
        StructuredOutputEs original = new()
        {
            Anuncio = new Anuncio
            {
                TipoDeOperación = TiposDeOperacion.venta,
                PrecioDelAnuncio = 125000m,
                NúmeroDeDormitorios = 3
            }
        };

        string json = JsonConvert.SerializeObject(original);
        StructuredOutputEs? result = JsonConvert.DeserializeObject<StructuredOutputEs>(json);

        Assert.Contains("\"tipo_de_operación\"", json);
        Assert.Contains("\"precio_del_anuncio\"", json);
        Assert.Contains("\"número_de_dormitorios\"", json);
        Assert.Equal(TiposDeOperacion.venta, result?.Anuncio?.TipoDeOperación);
        Assert.Equal(125000m, result?.Anuncio?.PrecioDelAnuncio);
        Assert.Equal(3, result?.Anuncio?.NúmeroDeDormitorios);
    }

    [Fact]
    public void GeneratedSchema_PreservesSpanishPropertyNames()
    {
        JObject schema = JObject.Parse(StructuredOutputSchema.GetJsonSchemaString());
        HashSet<string> names = schema
            .Descendants()
            .OfType<JProperty>()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Contains("tipo_de_operación", names);
        Assert.Contains("precio_del_anuncio", names);
        Assert.Contains("número_de_dormitorios", names);
    }
}