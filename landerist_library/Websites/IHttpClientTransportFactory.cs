namespace landerist_library.Websites;

public interface IHttpClientTransportFactory
{
    HttpClient Create(
        bool useProxy,
        TimeSpan timeout,
        bool allowAutoRedirect = true);
}
