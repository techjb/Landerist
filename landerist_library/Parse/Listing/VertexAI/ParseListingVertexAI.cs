using Google.Cloud.AIPlatform.V1;
using Google.Protobuf;
using landerist_library.Configuration;
using landerist_library.Parse.Listing.ChatGPT;
using landerist_library.Websites;
using static Google.Cloud.AIPlatform.V1.NearestNeighborQuery.Types;

namespace landerist_library.Parse.Listing.VertexAI
{
    public class ParseListingVertexAI
    {
        private static readonly string ModelName =
                            //"gemini-1.5-flash-preview-0514";
                            "gemini-1.5-pro-preview-0514";

        private static readonly string ProjectId = "landerist";

        private static readonly string Location = "us-central1";

        private static readonly string Publisher = "google";

        private static readonly string SystemPrompt =
            "Eres un clasificador de imágenes. " +
            "La imagen proporcionada por el usario es una captura de pantalla de una página web. " +
            "Tu tarea es determinar si la imagen es la página web de un único anuncio inmobiliario o no.";


        public static (PageType pageType, landerist_orels.ES.Listing? listing) Parse(Page page)
        {
            (PageType pageType, landerist_orels.ES.Listing? listing) result = (PageType.MayBeListing, null);
            if (page.Screenshot == null || page.Screenshot.Length == 0)
            {
                return result;
            }

            string? text = Parse(page.Screenshot).Result;
            Console.WriteLine(text);

            return result;
        }

        private static async Task<string?> Parse(byte[] screenshot)
        {
            try
            {
                var predictionServiceClient = GetPredictionServiceClient();
                var generateContentRequest = GetGenerateContentRequest(screenshot);
                GenerateContentResponse response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);

                var functionCall = response.Candidates[0].Content.Parts[0].FunctionCall;
                Console.WriteLine(functionCall);

                return response.Candidates[0].Content.Parts[0].Text;
            }
            catch (Exception ex)
            {

            }
            return null;
        }


        private static PredictionServiceClient GetPredictionServiceClient()
        {
            return new PredictionServiceClientBuilder
            {
                Endpoint = $"{Location}-aiplatform.googleapis.com",
                JsonCredentials = PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL,
            }.Build();
        }

        private static GenerateContentRequest GetGenerateContentRequest(byte[] screenshot)
        {
            return new GenerateContentRequest
            {
                Model = $"projects/{ProjectId}/locations/{Location}/publishers/{Publisher}/models/{ModelName}",
                Contents =
                {
                    new Content
                    {
                        Role = "USER",
                        Parts =
                        {
                            new Part
                            {
                                InlineData = new()
                                {
                                    MimeType = "image/png",
                                    Data = ByteString.CopyFrom(screenshot)
                                }
                            },
                            new Part {
                                Text = "Captura de pantalla"
                            },
                        }
                    }
                },
                GenerationConfig = new GenerationConfig()
                {
                    Temperature = 0,
                },
                Tools =
                {
                    new Tool
                    {
                        FunctionDeclarations =
                        {
                            IsListingFunctionDeclaration()
                        },                        
                    },
                },
                

                SystemInstruction = new Content
                {
                    Parts =
                    {
                        new Part
                        {
                            Text = SystemPrompt
                        }
                    }
                }
            };
        }

        private static FunctionDeclaration IsListingFunctionDeclaration()
        {
            return new FunctionDeclaration
            {
                Name = "resultado_de_la_consulta",
                Description = "Trata el resultado de la consulta",
                Parameters = new OpenApiSchema
                {
                    Type = Google.Cloud.AIPlatform.V1.Type.Object,
                    Properties =
                        {
                            ["es_un_anuncio"] = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                                Description = "true si a imagen es un anuncio, false en caso contrario"
                            },
                        },
                }
                //Name = ParseListingTool.FunctionNameIsListing,
                //Description = ParseListingTool.FunctionDescriptionIsListing,

                //Parameters = new OpenApiSchema
                //{
                //    Type = Google.Cloud.AIPlatform.V1.Type.Object,
                //    Properties =
                //    {
                //        ["es_un_anuncio"] = new()
                //        {
                //            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                //            Description = "La imagen es un anuncio"
                //        },                        
                //    },

                //}
            };
        }

        private static FunctionDeclaration IsNotListingFuncionDeclaration()
        {
            return new FunctionDeclaration
            {
                Name = ParseListingTool.FunctionNameIsNotListing,
                Description = ParseListingTool.FunctionDescriptionIsNotListing,
                //Parameters = new OpenApiSchema
                //{
                //    Type = Google.Cloud.AIPlatform.V1.Type.Object,
                //    Properties =
                //    {
                //        ["no_es_un_anuncio"] = new()
                //        {
                //            Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                //            Description = "La imagen no es un anuncio"
                //        },
                //    },

                //}
            };
        }

        public static void Test()
        {
            var d = TextInput().Result;
        }

        private static async Task<string> TextInput()
        {
            var predictionServiceClient = new PredictionServiceClientBuilder
            {
                Endpoint = $"{Location}-aiplatform.googleapis.com",
                JsonCredentials = PrivateConfig.GOOGLE_CLOUD_VERTEX_AI_CREDENTIAL
            }.Build();

            string prompt = @"What's a good name for a flower shop that specializes in selling bouquets of dried flowers?";

            var generateContentRequest = new GenerateContentRequest
            {
                Model = $"projects/{ProjectId}/locations/{Location}/publishers/{Publisher}/models/{ModelName}",
                Contents =
                {
                    new Content
                    {
                        Role = "USER",
                        Parts =
                        {
                            new Part { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig()
                {
                    Temperature = 0,
                },
                SystemInstruction = new Content
                {
                    Role = "USER",
                    Parts =
                        {
                            new Part { Text = SystemPrompt }
                        }
                }

            };

            GenerateContentResponse response = await predictionServiceClient.GenerateContentAsync(generateContentRequest);

            string responseText = response.Candidates[0].Content.Parts[0].Text;
            Console.WriteLine(responseText);

            return responseText;
        }
    }
}
