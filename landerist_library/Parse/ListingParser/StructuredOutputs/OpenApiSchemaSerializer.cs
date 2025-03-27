using Google.Cloud.AIPlatform.V1;

namespace landerist_library.Parse.ListingParser.StructuredOutputs
{
    public class OpenApiSchemaSerializer
    {
        public static object Serialize(OpenApiSchema schema)
        {
            var schemaJson = new Dictionary<string, object>
            {
                ["type"] = MapOpenApiType(schema)
            };

            if (!string.IsNullOrEmpty(schema.Description))
            {
                schemaJson["description"] = schema.Description;
            }

            if (!string.IsNullOrEmpty(schema.Format))
            {
                schemaJson["format"] = schema.Format;
            }
            if (schema.Nullable)
            {
                schemaJson["nullable"] = schema.Nullable;
            }

            if (schema.Enum != null && schema.Enum.Count > 0)
            {
                schemaJson["enum"] = schema.Enum;
            }

            if (schema.Properties != null && schema.Properties.Count > 0)
            {
                var properties = new Dictionary<string, object>();

                foreach (var property in schema.Properties)
                {
                    properties[property.Key] = Serialize(property.Value);
                }

                schemaJson["properties"] = properties;
            }

            if (schema.Items != null)
            {
                schemaJson["items"] = Serialize(schema.Items);
            }

            if (schema.Required != null && schema.Required.Count > 0)
            {
                schemaJson["required"] = schema.Required;
            }

            return schemaJson;
        }

        private static string MapOpenApiType(OpenApiSchema schema)
        {
            return schema.Type switch
            {
                Google.Cloud.AIPlatform.V1.Type.Boolean => "boolean",
                Google.Cloud.AIPlatform.V1.Type.Integer => "integer",
                Google.Cloud.AIPlatform.V1.Type.String => "string",
                Google.Cloud.AIPlatform.V1.Type.Number => "number",
                Google.Cloud.AIPlatform.V1.Type.Object => "object",
                Google.Cloud.AIPlatform.V1.Type.Array => "array",
                _ => "string"
            };
        }
    }
}
