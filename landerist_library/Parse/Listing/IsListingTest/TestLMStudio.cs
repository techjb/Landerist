using Newtonsoft.Json;
using System.Data;
using System.Text;

namespace landerist_library.Parse.Listing.IsListingTest
{
    public class TestLMStudio : IsListingTest
    {
        private const string IMStudioUrl = "http://localhost:1234/v1/chat/completions";

        public static readonly string SystemMessage =
            "Un anuncio completo de oferta inmobiliaria debe contener la siguiente información: " +
            "1. Tipo de propiedad (por ejemplo, casa, apartamento, terreno, etc.). " +
            "2. Ubicación (puede ser la ciudad, barrio o dirección exacta). " +
            "3. Precio de venta o alquiler. " +
            "4. Descripción detallada de la propiedad (número de habitaciones, baños, tamaño en metros cuadrados, etc.). " +
            "Evalúa el texto introducido por el usuario y determina si contiene todos los datos completos de un anuncio de oferta inmobiliaria. " +
            "Asegúrate de identificar la presencia de cada uno de los puntos anteriores en el texto. " +
            "Response sólo con 'si' o 'no'. No des ninguna explicación."
            ;
        public static void Start()
        {
            var dataTable = GetListingsAndNoListings(100);
            TestLmStudio(dataTable);
        }

        private static void TestLmStudio(DataTable dataTable)
        {
            Console.WriteLine("Testing data ..");
            Total = dataTable.Rows.Count;
            foreach (DataRow row in dataTable.Rows)
            {
                var testResult = Test(row);
                if (testResult is null)
                {
                    Errors++;
                }
                else
                {
                    Processed++;
                    if ((bool)testResult)
                    {
                        Sucess++;
                    }
                    else
                    {
                        NoSucess++;
                    }
                }                
                OutputConsole();
            }
        }

        private static bool? Test(DataRow row)
        {
            string text = (string)row["ResponseBodyText"];
            int label = (int)row["label"];
            bool? isListing = IsListing(text).GetAwaiter().GetResult();
            if (isListing is null)
            {
                return null;
            }
            if ((bool)isListing && label.Equals(1))
            {
                return true;
            }
            if (!(bool)isListing && label.Equals(0))
            {
                return true;
            }
            return false;
        }

        private static async Task<bool?> IsListing(string text)
        {
            var jsonContent =
                "{ \"messages\": [ " +
                "   { \"role\": \"system\", \"content\": \"" + SystemMessage + "\" }, " +
                "   { \"role\": \"user\", \"content\": \"" + text + "\" } ], " +
                "\"temperature\": 0, " +
                "\"max_tokens\":2, " +
                "\"n_threads\":4, " +
                //"\"n_gpu_layers\":20, " +
                "\"stream\": false " +
                "}";

            using var client = new HttpClient();
            var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            try
            {
                var httpResponse = await client.PostAsync(IMStudioUrl, stringContent);
                if (httpResponse.IsSuccessStatusCode)
                {
                    var responseContent = await httpResponse.Content.ReadAsStringAsync();
                    if (responseContent != null)
                    {
                        LMStudioResponse? lMStudioResponse = JsonConvert.DeserializeObject<LMStudioResponse>(responseContent);
                        if (lMStudioResponse != null)
                        {
                            if (lMStudioResponse.Choices != null && lMStudioResponse.Choices.Count.Equals(1))
                            {
                                var content = lMStudioResponse.Choices[0].Message.Content;
                                return content.ToLower().Trim().Equals("si");
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return null;
        }
    }
}
