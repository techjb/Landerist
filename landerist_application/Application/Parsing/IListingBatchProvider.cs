using landerist_library.Application.Pages;
using landerist_library.Pages;

namespace landerist_library.Application.Parsing;

public interface IListingBatchProvider
{
    (string? fileSuccess, string? fileError)? GetFiles(string batchId);
    string? DownloadFile(string file);
    (Page page, string? text)? ReadLine(string batchId, string line, IPageCatalog pages);
}
