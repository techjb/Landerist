using landerist_library.Pages;
using landerist_orels.ES;

namespace landerist_library.Application.Scraping;

public sealed record PageClassificationResult(
    PageType? PageType,
    Listing? Listing,
    bool WaitingAiRequest);
