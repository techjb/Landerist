using landerist_library.Websites;
using Newtonsoft.Json;

namespace landerist_library.Parse.Listing.ChatGPT
{
    public class ChatGPTParseListing : ChatGPTRequest
    {

        public static string SystemMessage =
            "Proporciona una representación JSON que siga estrictamente este esquema:\n\n" +
            ChatGPTResponseSchema.GetSchema() + "\n\n" +
            "Escribe null en los campos que falten.";

        public ChatGPTParseListing() : base(SystemMessage)
        {

        }

        public landerist_orels.ES.Listing? Parse(Page page)
        {
            var response = GetResponse(page.ResponseBodyText, true);
            if (!string.IsNullOrEmpty(response))
            {
                try
                {
                    ChatGPTResponse? listingResponse = JsonConvert.DeserializeObject<ChatGPTResponse>(response);
                    if (listingResponse != null)
                    {
                        return listingResponse.ToListing(page);
                    }
                }
                catch //(Exception exception)
                {
                    //Logs.Log.WriteLogErrors(Page.Uri, exception);
                }
            }
            return null;
        }
    }
}
