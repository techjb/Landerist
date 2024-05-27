using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;

namespace landerist_library.Parse.Listing
{
    public class ParseListing
    {
        public static (PageType pageType, landerist_orels.ES.Listing? listing) ParseText(Page page)
        {
            var userInput = UserTextInput.GetText(page);
            var chatResponse = ChatGPTRequest.GetResponse(userInput);
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (chatResponse == null)
            {
                return result;
            }

            var tool = ChatGPTRequest.GetTool(chatResponse);
            if (tool == null)
            {
                return result;
            }
            switch (tool.Function.Name)
            {
                case ChatGPTTools.FunctionNameIsNotListing:
                    {
                        result.pageType = PageType.NotListingByParser;
                    }
                    break;
                case ChatGPTTools.FunctionNameIsListing:
                    {
                        result = ChatGPTRequest.ParseListing(page, tool);
                    }
                    break;
            }

            return result;
        }
    }
}
