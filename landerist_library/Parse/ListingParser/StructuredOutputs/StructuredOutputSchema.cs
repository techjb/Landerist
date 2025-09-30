using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.ComponentModel.DataAnnotations;


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

        public static string GetJsonSchemaString()
        {
            JSchema jSChema = GetJsonSchema();
            SetSchemaVersion(jSChema);
            //SetAllOf(jSChema); // problems in vllm
            ParsePropertyType(jSChema);
            SetAdditionalPropertiesFalse(jSChema);
            return jSChema.ToString(SchemaVersion.Draft7);
        }

        public static JSchema GetJsonSchema()
        {
            return JSchemaGenerator.Generate(typeof(StructuredOutputEs));            
        }

        private static void ParsePropertyType(JSchema jSChema)
        {
            if (!jSChema.Properties[StructuredOutputEsJson.FunctionNameListing].Properties.TryGetValue(nameof(StructuredOutputEsJson.tipo_de_inmueble), out var propertyTypeSchema))
            {
                return;
            }

            if (propertyTypeSchema.Enum != null && propertyTypeSchema.Enum.Count > 0)
            {
                propertyTypeSchema.OneOf.Clear();
                foreach (var literal in propertyTypeSchema.Enum.OfType<JValue>())
                {
                    var valueStr = literal.Value?.ToString();
                    if (valueStr == null)
                    {
                        continue;
                    }

                    var fieldInfo = typeof(TiposDeInmueble).GetField(valueStr);
                    string? desc = null;
                    if (fieldInfo != null)
                    {
                        desc = fieldInfo
                            .GetCustomAttributes(typeof(DisplayAttribute), false)
                            .OfType<DisplayAttribute>()
                            .FirstOrDefault()
                            ?.Description;
                    }

                    propertyTypeSchema.OneOf.Add(new JSchema
                    {
                        Const = new JValue(literal.Value),
                        //Title = valueStr,
                        Description = desc
                    });
                }
                propertyTypeSchema.Enum.Clear();
                propertyTypeSchema.Type = null;
            }
        }

        private static void SetSchemaVersion(JSchema jSchema)
        {
            jSchema.SchemaVersion = new Uri("http://json-propertyTypeSchema.org/draft-07/propertyTypeSchema#");
        }
        private static void SetAllOf(JSchema jSchema)
        {
            var condition = new JSchema
            {
                If = new JSchema
                {
                    Properties =
                    {
                        [StructuredOutputEsJson.FunctionNameIsListing] = new JSchema
                        {
                            Enum = { new JValue(true) }
                        }
                    }
                },
                Then = new JSchema
                {
                    Required = { StructuredOutputEsJson.FunctionNameListing }
                }
            };
            jSchema.AllOf.Add(condition);
        }

        private static void SetAdditionalPropertiesFalse(JSchema jSchema)
        {
            if ((jSchema.Type & JSchemaType.Object) == JSchemaType.Object)
            {
                jSchema.AllowAdditionalProperties = false;
            }

            foreach (var propertySchema in jSchema.Properties.Values)
            {
                SetAdditionalPropertiesFalse(propertySchema);
            }

            if (jSchema.Items != null)
            {
                foreach (var itemSchema in jSchema.Items)
                {
                    SetAdditionalPropertiesFalse(itemSchema);
                }
            }

            if (jSchema.AnyOf != null)
            {
                foreach (var subschema in jSchema.AnyOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }

            if (jSchema.AllOf != null)
            {
                foreach (var subschema in jSchema.AllOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }

            if (jSchema.OneOf != null)
            {
                foreach (var subschema in jSchema.OneOf)
                {
                    SetAdditionalPropertiesFalse(subschema);
                }
            }
        }
    }
}
