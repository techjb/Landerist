using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using landerist_library.Configuration;
using landerist_library.Logs;

namespace landerist_library.Export
{
    public class S3
    {
        private readonly AmazonS3Client AmazonS3Client;

        public S3()
        {
            AmazonS3Client = new AmazonS3Client(Config.AWS_ACESSKEYID, Config.AWS_SECRETACCESSKEY, RegionEndpoint.EUWest3);
        }

        public bool UploadFilePublicBucket(string file, string key)
        {
            return UploadFile(file, key, Config.AWS_S3_DOWNLOADS_BUCKET, string.Empty);
        }

        public bool UploadToDownloadsBucket(string file, string key, string subdirectoryInBucket)
        {
            return UploadFile(file, key, Config.AWS_S3_DOWNLOADS_BUCKET, subdirectoryInBucket);
        }

        public bool UploadToWebsiteBucket(string file, string key, string subdirectoryInBucket)
        {
            return UploadFile(file, key, Config.AWS_S3_WEBSITE_BUCKET, subdirectoryInBucket);
        }

        public bool UploadFile(string file, string key, string bucketName)
        {
            return UploadFile(file, key, bucketName, string.Empty);
        }

        public bool UploadFile(string file, string key, string bucketName, string subdirectoryInBucket)
        {
            bool success = false;
            if (!File.Exists(file))
            {
                return success;
            }

            if (!string.IsNullOrEmpty(subdirectoryInBucket))
            {
                key = subdirectoryInBucket + @"/" + key;
            }

            TransferUtilityUploadRequest transferUtilityUploadRequest = new()
            {
                BucketName = bucketName,
                Key = key,
                FilePath = file
            };

            TransferUtility transferUtitlity = new(AmazonS3Client);

            try
            {
                transferUtitlity.Upload(transferUtilityUploadRequest);
                success = true;
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("S3 UploadFile", exception);
            }
            transferUtitlity.Dispose();
            return success;
        }

        public async Task<List<S3Object>> ListObjects(string bucketName, string directory = "")
        {
            List<S3Object> list = [];

            ListObjectsV2Request listObjectsV2Request = new()
            {
                BucketName = bucketName
            };
            if (!directory.Equals(string.Empty))
            {
                listObjectsV2Request.Prefix = directory;
            }

            var listObjectsV2Response = new ListObjectsV2Response();
            try
            {
                do
                {
                    listObjectsV2Response = await AmazonS3Client.ListObjectsV2Async(listObjectsV2Request);
                    list.AddRange(listObjectsV2Response.S3Objects);
                    listObjectsV2Request.ContinuationToken = listObjectsV2Response.NextContinuationToken;
                }
                while (listObjectsV2Response.IsTruncated);
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("S3 ListObjects", exception);
            }
            return list;
        }

        public async Task<List<DeletedObject>> DeleteObjects(string bucketName, List<string> objectKeys)
        {
            List<DeletedObject> list = [];
            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = new List<KeyVersion>(objectKeys.ConvertAll(k => new KeyVersion() { Key = k })),
                Quiet = true
            };
            try
            {
                var response = await AmazonS3Client.DeleteObjectsAsync(deleteObjectsRequest);
                list = response.DeletedObjects;
            }
            catch (Exception exception)
            {
                Log.WriteLogErrors("S3 DeleteObjects", exception);
            }

            return list;
        }

        public (DateTime? lastModified, long? contentLength) GetFileInfo(string bucketName, string objectKey)
        {
            try
            {
                var metadataResponse = AmazonS3Client.GetObjectMetadataAsync(bucketName, objectKey).Result;
                var lastModified = metadataResponse.LastModified;
                var contentLength = metadataResponse.ContentLength; // Tamaño en bytes                

                return (lastModified, contentLength);
            }
            catch (AmazonS3Exception e)
            {
                Log.WriteLogErrors("S3 GetFileInfo", e);
            }
            return (null, null);
        }
    }
}
