using landerist_orels.ES;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;


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

        public static string GetJsonSchema()
        {
            JSchema jSChema = JSchemaGenerator.Generate(typeof(StructuredOutputEs));
            SetSchemaVersion(jSChema);
            SetAllOf(jSChema);
            SetAdditionalPropertiesFalse(jSChema);
            return jSChema.ToString(SchemaVersion.Draft7)
                //.Replace("\"definitions\"", "\"$defs\"")
                //.Replace("#/definitions/", "#/$defs/")
                ;
        }

        public static string GetJsonSchema2()
        {
            JSchema jSChema = JSchemaGenerator.Generate(typeof(StructuredOutputEs2));
            
            SetSchemaVersion(jSChema);
            SetAllOf(jSChema);            
            ParsePropertyType(jSChema);
            SetAdditionalPropertiesFalse(jSChema);
            string schema = jSChema.ToString(SchemaVersion.Draft7);
            return schema;
        }

        public static string GetJsonSchema3()
        {
            JSchema jSChema = JSchemaGenerator.Generate(typeof(StructuredOutputEs));
            //SetSchemaVersion(jSChema);
            //SetAllOf(jSChema);
            SetAdditionalPropertiesFalse(jSChema);
            return jSChema.ToString(SchemaVersion.Draft7);
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

                    var fieldInfo = typeof(Anuncio.TiposDeInmueble).GetField(valueStr);
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
