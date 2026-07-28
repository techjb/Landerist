using landerist_library.Application.Tasks;

namespace landerist_library.Infrastructure.Tasks;

public sealed class TenMinuteTaskJob : IRecurringTaskJob
{
    private readonly TaskBatchDownload _download;
    private readonly TaskBatchUpload _upload;

    public TenMinuteTaskJob(
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
