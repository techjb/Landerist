using landerist_library.Pages;

namespace landerist_library.Application.Parsing;

public interface IListingInputPreparer
{
    void Prepare(Page page);

    bool MatchesUnavailableRule(Page page);
}
