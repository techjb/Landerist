using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace landerist_library.Parse.CadastralReference
{
    public class Address
    {
        [JsonProperty("localid")] 
                                      
        public required string LocalId { get; set; }

        [JsonProperty("address")]        
        public required string AddressValue { get; set; }
    }

    public class AddressList
    {
        [JsonProperty("addresses")]        
        public required List<Address> Addresses { get; set; }
    }
}
