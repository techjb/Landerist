using landerist_library.Application.Tasks;
using landerist_library.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class LegacyTenMinuteTaskJob : IRecurringTaskJob
{
    private readonly TaskBatchDownload _download;
    private readonly TaskBatchUpload _upload;

    public LegacyTenMinuteTaskJob(
        TaskBatchDownload download,
        TaskBatchUpload upload)
    {
        ArgumentNullException.ThrowIfNull(download);
        ArgumentNullException.ThrowIfNull(upload);
        _download = download;
        _upload = upload;
    }

    public void Run()
    {
        _download.Start();
        _upload.Start();
    }
}
