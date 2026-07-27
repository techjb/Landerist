using landerist_library.Websites;

namespace landerist_library.Application.Persistence;

public sealed class WebsitePersistenceService : IWebsitePersistenceService
{
    private readonly IWebsiteRepository _repository;

    public WebsitePersistenceService(IWebsiteRepository repository)
    {
        ArgumentNullException.ThrowIfNull(repository);
        _repository = repository;
    }

    public bool Insert(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return _repository.Insert(website);
    }

    public bool Update(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return _repository.Update(website);
    }

    public bool Delete(Website website)
    {
        ArgumentNullException.ThrowIfNull(website);
        return _repository.Delete(website.Host);
    }
}