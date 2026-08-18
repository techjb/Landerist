using Amazon;
using Amazon.CloudFront;
using Amazon.CloudFront.Model;
using landerist_library.Logs;

namespace landerist_library.Infrastructure.Distribution.Cloud;

public sealed class CloudFrontCdnInvalidator(
    string accessKeyId,
    string secretAccessKey,
    string distributionId) : ICdnInvalidator
{
    public bool InvalidateAll()
    {
        using AmazonCloudFrontClient client = new(
            accessKeyId,
            secretAccessKey,
            RegionEndpoint.EUWest3);
        CreateInvalidationRequest request = new()
        {
            DistributionId = distributionId,
            InvalidationBatch = new InvalidationBatch
            {
                CallerReference = DateTime.UtcNow.Ticks.ToString(),
                Paths = new Paths { Quantity = 1, Items = ["/*"] }
            }
        };

        try
        {
            _ = client.CreateInvalidationAsync(request).GetAwaiter().GetResult();
            return true;
        }
        catch (Exception exception)
        {
            Log.WriteError(nameof(CloudFrontCdnInvalidator), exception);
            return false;
        }
    }
}
