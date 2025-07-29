using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace landerist_library.Parse.ListingParser.StructuredOutputs
{
    public class StructuredOutputSchema
    {
        private static readonly JSchemaGenerator JSchemaGenerator = new()
        {
            DefaultRequired = Required.AllowNull,
            GenerationProviders =
            {
                new StringEnumGenerationProvider(),
            }
        };

        public static string GetSchema()
        {
            JSchema jSChema = JSchemaGenerator.Generate(typeof(StructuredOutputEs));
            SetAdditionalPropertiesFalse(jSChema);
            return jSChema.ToString();
        }

        private static void SetAdditionalPropertiesFalse(JSchema schema)
        {
            if ((schema.Type & JSchemaType.Object) == JSchemaType.Object)
            {
                schema.AllowAdditionalProperties = false;
            }

            foreach (var propertySchema in schema.Properties.Values)
            {
                SetAdditionalPropertiesFalse(propertySchema);
            }

            if (schema.Items != null)
            {
                foreach (var itemSchema in schema.Items)
                {
                    SetAdditionalPropertiesFalse(itemSchema);
                }
            }

            if (schema.AnyOf != null)
            {
                foreach (var subschema in schema.AnyOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }

            if (schema.AllOf != null)
            {
                foreach (var subschema in schema.AllOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }

            if (schema.OneOf != null)
            {
                foreach (var subschema in schema.OneOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }
        }
    }
}
