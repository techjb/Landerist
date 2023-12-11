using Amazon;
using Amazon.S3;
using Amazon.S3.Transfer;
using landerist_library.Configuration;

namespace landerist_library.Export
{
    public class S3
    {
        private readonly AmazonS3Client AmazonS3Client;
        public S3()
        {
            AmazonS3Client = new AmazonS3Client(Config.AWS_ACESSKEYID, Config.AWS_SECRETACCESSKEY, RegionEndpoint.EUWest3);
        }

        public bool UploadFile(string file, string key)
        {
            return UploadFile(file, key, Config.AWS_S3_PUBLIC_BUCKET, string.Empty);
        }

        public bool UploadFile(string file, string key, string bucketName)
        {
            return UploadFile(file, key, bucketName, string.Empty);
        }

        public bool UploadFile(string file, string key, string bucketName, string subDirectoryInBucket)
        {
            bool success = false;
            if (!File.Exists(file))
            {
                return success;
            }
            // todo: not tested
            if (!string.IsNullOrEmpty(subDirectoryInBucket))
            {
                key = subDirectoryInBucket + @"/" + key;
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
                Console.WriteLine("Upload: " + exception.Message);
            }
            transferUtitlity.Dispose();
            return success;
        }
    }
}
