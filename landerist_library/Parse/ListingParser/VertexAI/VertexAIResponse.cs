using Google.Cloud.AIPlatform.V1;

namespace landerist_library.Parse.ListingParser.VertexAI
{
    public class VertexAIResponse
    {
        public static string? GetResponseText(GenerateContentResponse response)
        {
            try
            {
                if (response.Candidates != null)
                {
                    var candidate = response.Candidates[0];
                    return GetResponseText(candidate);
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("VertexAIResponse GetResponseText", response.ToString(), exception);
            }
            return null;
        }

        public static string? GetResponseText(Candidate candidate)
        {
            try
            {
                if (candidate.Content != null &&
                    candidate.Content.Parts != null)
                {
                    return candidate.Content.Parts[0].Text;
                }
            }
            catch (Exception exception)
            {
                Logs.Log.WriteError("VertexAIResponse GetResponseText", candidate.ToString(), exception);
            }
            return null;
        }
    }
}
