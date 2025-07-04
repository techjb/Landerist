﻿using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Grpc.Core;
using landerist_library.Configuration;
using landerist_library.Logs;

namespace landerist_library.Export
{
    public class S3
    {
        private readonly AmazonS3Client AmazonS3Client;

        public S3()
        {
            AmazonS3Client = new AmazonS3Client(PrivateConfig.AWS_ACESSKEYID, PrivateConfig.AWS_SECRETACCESSKEY, RegionEndpoint.EUWest3);
        }


        public bool UploadToDownloadsBucket(string file, string key, string subdirectoryInBucket, List<(string, string)>? metadata = null)
        {
            return UploadFile(file, key, PrivateConfig.AWS_S3_DOWNLOADS_BUCKET, subdirectoryInBucket, metadata);
        }

        public bool UploadToWebsiteBucket(string file, string key, string subdirectoryInBucket)
        {
            return UploadFile(file, key, PrivateConfig.AWS_S3_WEBSITE_BUCKET, subdirectoryInBucket);
        }

        public bool UploadFile(string file, string key, string bucketName)
        {
            return UploadFile(file, key, bucketName, string.Empty);
        }


        public bool UploadFile(string file, string key, string bucketName, string subdirectoryInBucket, List<(string, string)>? metadata = null)
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
                FilePath = file,
            };

            if (metadata != null && metadata.Count > 0)
            {
                foreach (var (metadataKey, metadataValue) in metadata)
                {
                    if (!string.IsNullOrEmpty(metadataKey) && !string.IsNullOrEmpty(metadataValue))
                    {
                        transferUtilityUploadRequest.Metadata.Add(metadataKey, metadataValue);
                    }
                }
            }

            TransferUtility transferUtitlity = new(AmazonS3Client);

            try
            {
                transferUtitlity.Upload(transferUtilityUploadRequest);
                success = true;
            }
            catch (Exception exception)
            {
                Log.WriteError("S3 UploadFile", exception);
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

            try
            {
                ListObjectsV2Response? listObjectsV2Response;
                do
                {
                    listObjectsV2Response = await AmazonS3Client.ListObjectsV2Async(listObjectsV2Request);
                    list.AddRange(listObjectsV2Response.S3Objects);
                    listObjectsV2Request.ContinuationToken = listObjectsV2Response.NextContinuationToken;
                }
                while ((bool)listObjectsV2Response.IsTruncated!);
            }
            catch (Exception exception)
            {
                Log.WriteError("S3 ListObjects", exception);
            }
            return list;
        }

        public async Task<List<DeletedObject>> DeleteObjects(string bucketName, List<string> objectKeys)
        {
            List<DeletedObject> list = [];
            var deleteObjectsRequest = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = [.. objectKeys.ConvertAll(k => new KeyVersion() { Key = k })],
                Quiet = true
            };
            try
            {
                var response = await AmazonS3Client.DeleteObjectsAsync(deleteObjectsRequest);
                if (response != null && response.DeletedObjects != null)
                {
                    list = response.DeletedObjects;
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("S3 DeleteObjects", exception);
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
                Log.WriteError("S3 GetFileInfo", e);
            }
            return (null, null);
        }

        public string? GetMetadataValue(string bucketName, string objectKey, string metaDataKey)
        {
            metaDataKey = "x-amz-meta-" + metaDataKey;
            try
            {
                var metadataResponse = AmazonS3Client.GetObjectMetadataAsync(bucketName, objectKey).Result;
                if (metadataResponse.Metadata.Keys.Any(k => k.Equals(metaDataKey, StringComparison.OrdinalIgnoreCase)))
                {
                    return metadataResponse.Metadata[metaDataKey];
                }
            }
            catch (AmazonS3Exception e)
            {
                Log.WriteError("S3 GetFileInfo", e);
            }
            return null;
        }
    }
}
