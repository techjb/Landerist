﻿using Google.Cloud.AIPlatform.V1;
using landerist_library.Configuration;
using System.Text.Json.Serialization;
using System.Text.Json;
using landerist_library.Websites;
using landerist_library.Logs;

namespace landerist_library.Parse.ListingParser.VertexAI.Batch
{
    public class VertexAIBatchDownload
    {
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false) },
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };


        public static List<string>? GetFiles(string batchId)
        {
            try
            {
                var batchPredictionJob = BatchPredictions.GetBatchPredictionJob(batchId);
                if (batchPredictionJob != null && batchPredictionJob.State.Equals(JobState.Succeeded))
                {
                    string file = batchPredictionJob.OutputInfo.GcsOutputDirectory.
                        Replace("gs://" + PrivateConfig.GOOGLE_CLOUD_BUCKET_NAME + "/", "") + "/predictions.jsonl";

                    return [file];
                }
            }
            catch (Exception exception)
            {
                Log.WriteError("VertexAIBatchDownload GetFiles", exception);
            }
            return null;
        }

        public static string? DownloadFile(string file)
        {
            string outputFilePath = Config.BATCH_DIRECTORY + file.Split("/")[1];
            File.Delete(outputFilePath);
            if (CloudStorage.DownloadFile(file, outputFilePath))
            {
                return outputFilePath;
            }
            return null;
        }

        public static (Page page, string? text)? ReadLine(string line)
        {
            VertexAIBatchResponse? vertexAIBatchResponse = null;
            try
            {
                vertexAIBatchResponse = JsonSerializer.Deserialize<VertexAIBatchResponse>(line, JsonSerializerOptions);
            }
            catch (Exception exception)
            {
                Log.WriteError("VertexAIBatchDownload ReadLine", exception);
            }
            if (vertexAIBatchResponse is null)
            {
                Log.WriteError("VertextAIBatchDownload ReadLine", "vertexAIBatchResponse is null");
                return null;
            }
            Page? page = GetPage(vertexAIBatchResponse);
            if (page == null)
            {
                Log.WriteError("VertextAIBatchDownload ReadLine", "page is null");
                return null;
            }
            if (!IsValidResponse(vertexAIBatchResponse))
            {
                var finishReason = GetFinishReason(vertexAIBatchResponse);
                Log.WriteError("VertextAIBatchDownload ReadLine", "response is not valid finishReason: " + finishReason ?? "");
                return (page, null);
            }
            string? text = GetText(vertexAIBatchResponse);
            if (string.IsNullOrEmpty(text))
            {
                Log.WriteError("VertextAIBatchDownload ReadLine", "text is null");
            }
            return (page, text);
        }

        private static bool IsValidResponse(VertexAIBatchResponse vertexAIBatchResponse)
        {
            var finishReason = GetFinishReason(vertexAIBatchResponse);
            if (finishReason != null)
            {
                return finishReason.Equals("STOP");
            }
            return false;
        }

        private static string? GetFinishReason(VertexAIBatchResponse vertexAIBatchResponse)
        {
            if (vertexAIBatchResponse.Response.Candidates != null)
            {
                return vertexAIBatchResponse.Response.Candidates[0].FinishReason;
            }
            return null;
        }

        private static string? GetText(VertexAIBatchResponse vertexAIBatchResponse)
        {
            if (vertexAIBatchResponse.Response.Candidates != null)
            {
                var content = vertexAIBatchResponse.Response.Candidates[0].Content;
                if (content != null && content.Parts != null)
                {
                    return content.Parts[0].Text;
                }
                else
                {
                    string finishReason = vertexAIBatchResponse.Response.Candidates[0].FinishReason;
                    Log.WriteError("VertexAIBatchDownload GetText", "FinishReason: " + finishReason);
                }
            }
            return null;
        }

        private static Page? GetPage(VertexAIBatchResponse vertexAIBatchResponse)
        {
            if (vertexAIBatchResponse.Request != null)
            {
                var labels = vertexAIBatchResponse.Request.labels;
                if (labels.TryGetValue("custom_id", out string? label))
                {
                    return Pages.GetPage(label);
                }
            }
            return null;
        }
    }
}
