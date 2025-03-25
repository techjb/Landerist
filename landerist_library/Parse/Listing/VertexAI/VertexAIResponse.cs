using Google.Cloud.AIPlatform.V1;

namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAIResponse
    {
        public static string? GetResponseText(GenerateContentResponse response)
        {
            try
            {
                if (response.Candidates != null &&
                    response.Candidates[0].Content != null &&
                    response.Candidates[0].Content.Parts != null)
                {
                    return response.Candidates[0].Content.Parts[0].Text;
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("VertexAIResponse GetResponseText", response.ToString(), exception);
            }
            return null;
        }
    }
}
