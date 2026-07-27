using landerist_library.Pages;

namespace landerist_library.Application.Scraping;

public interface IPageContentClassifier
{
    PageClassificationResult Classify(Page page);
}
