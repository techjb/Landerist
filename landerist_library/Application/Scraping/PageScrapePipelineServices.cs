namespace landerist_library.Application.Scraping;

public sealed class PageScrapePipelineServices
{
    public PageScrapePipelineServices(
        IPageAcquisitionService acquisition,
        IPageContentClassifier classifier,
        IPageIndexingService indexing,
        IPageSchedulingService scheduling)
    {
        ArgumentNullException.ThrowIfNull(acquisition);
        ArgumentNullException.ThrowIfNull(classifier);
        ArgumentNullException.ThrowIfNull(indexing);
        ArgumentNullException.ThrowIfNull(scheduling);

        Acquisition = acquisition;
        Classifier = classifier;
        Indexing = indexing;
        Scheduling = scheduling;
    }

    public IPageAcquisitionService Acquisition { get; }

    public IPageContentClassifier Classifier { get; }

    public IPageIndexingService Indexing { get; }

    public IPageSchedulingService Scheduling { get; }
}
