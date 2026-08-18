using System.Data;

namespace landerist_library.Application.Distribution;

public interface IWebsiteExportSource
{
    DataTable GetAll();
}
