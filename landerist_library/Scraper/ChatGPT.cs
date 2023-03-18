using OpenAI;
using OpenAI.Models.ChatCompletion;

namespace landerist_library.Scraper
{
    public class ChatGPT
    {
        private readonly OpenAiClient OpenAiClient = new(Config.OPENAI_API_KEY);

        string systemText =
            "Eres un clasificador de textos. Si el texto introducido contiene los datos de venta o alquiler " +
            "de un sólo inmueble, con su tipología, precio y localización, responde 'si'. " +
            "Responde 'no' en caso contrario. Si no lo sabes responde 'null'. Solo responde con una palabra.";

        public async Task<bool?> IsAdvertisement(string responseBodyText)
        {
            var userMessage = Dialog.StartAsSystem(systemText).ThenUser(responseBodyText);
            ChatCompletionRequest chatCompletionRequest = new()
            {
                Model = ChatCompletionModels.Gpt35Turbo,
                //MaxTokens = ChatCompletionRequest.MaxTokensDefault, // actualizar el paquete nuget, debería de salir.
                Temperature = 0,
                Messages = userMessage.GetMessages(),
            };

            try
            {
                var chatCompletionResponse = await OpenAiClient.GetChatCompletions(chatCompletionRequest);
                string responseMessage = chatCompletionResponse.Choices[0].Message!.Content;
                responseMessage = responseMessage.ToLower().Replace(".", string.Empty);

                if (responseMessage.Equals("si"))
                {
                    return true;
                }
                if (responseMessage.Equals("no"))
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

            }

            return null;
        }
    }
}
