﻿using landerist_library.Parse.ListingParser.StructuredOutputs;
using OpenAI;
using System.Text.Json.Nodes;

namespace landerist_library.Parse.ListingParser.OpenAI
{
    public class OpenAITools : StructuredOutputEsJson
    {
        public List<Tool> GetTools()
        {
            var functionIsListing = GetToolIsListing();
            var functionIsNotListing = GetToolIsNotListing();

            return [functionIsListing, functionIsNotListing];
        }

        private Tool GetToolIsListing()
        {
            var parameters = GetJsonObject();
            return new Function(FunctionNameIsListing, FunctionNameIsListingDescription, parameters);
        }

        public JsonObject GetJsonObject()
        {
            var properties = new JsonObject();

            AddString(properties, nameof(fecha_de_publicación));
            AddEnum(properties, nameof(tipo_de_operación), TiposDeOperación);
            AddEnum(properties, nameof(tipo_de_inmueble), TiposDeInmueble);
            AddEnum(properties, nameof(subtipo_de_inmueble), SubtiposDeInmueble);
            AddNumber(properties, nameof(precio_del_anuncio));
            AddString(properties, nameof(descripción_del_anuncio));
            AddString(properties, nameof(referencia_del_anuncio));
            AddString(properties, nameof(nombre_de_contacto));
            AddString(properties, nameof(teléfono_de_contacto));
            AddString(properties, nameof(email_de_contacto));
            AddString(properties, nameof(dirección_del_inmueble));
            AddString(properties, nameof(referencia_catastral));
            AddNumber(properties, nameof(tamaño_del_inmueble));
            AddNumber(properties, nameof(tamaño_de_la_parcela));
            AddNumber(properties, nameof(año_de_construcción));
            AddEnum(properties, nameof(estado_de_la_construcción), EstadosDeLaConstrucción);
            AddNumber(properties, nameof(plantas_del_edificio));
            AddString(properties, nameof(planta_del_inmueble));
            AddNumber(properties, nameof(número_de_dormitorios));
            AddNumber(properties, nameof(número_de_baños));
            AddNumber(properties, nameof(número_de_parkings));
            AddBoolean(properties, nameof(tiene_terraza));
            AddBoolean(properties, nameof(tiene_jardín));
            AddBoolean(properties, nameof(tiene_garaje));
            AddBoolean(properties, nameof(tiene_parking_para_moto));
            AddBoolean(properties, nameof(tiene_piscina));
            AddBoolean(properties, nameof(tiene_ascensor));
            AddBoolean(properties, nameof(tiene_acceso_para_discapacitados));
            AddBoolean(properties, nameof(tiene_trastero));
            AddBoolean(properties, nameof(está_amueblado));
            AddBoolean(properties, nameof(no_está_amueblado));
            AddBoolean(properties, nameof(tiene_calefacción));
            AddBoolean(properties, nameof(tiene_aire_acondicionado));
            AddBoolean(properties, nameof(permite_mascotas));
            AddBoolean(properties, nameof(tiene_sistemas_de_seguridad));

            return new JsonObject()
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray { }
            };
        }

        private static Tool GetToolIsNotListing()
        {
            var properties = new JsonObject();
            JsonArray NoEsUnAnuncio =
            [
                "NO_ES_UN_ANUNCIO"
            ];

            Add(properties, FunctionNameIsNotListing, "string", FunctionDescriptionIsNotListing, NoEsUnAnuncio);

            var parameters = new JsonObject()
            {
                ["type"] = "object",
                ["properties"] = properties,
                ["required"] = new JsonArray { }
            };

            return new Function(FunctionNameIsNotListing, FunctionDescriptionIsNotListing, parameters);
        }

        private static void AddString(JsonObject jsonObject, string name)
        {
            Add(jsonObject, name, "string");
        }

        private static void AddEnum(JsonObject jsonObject, string name, JsonArray jsonArray)
        {
            Add(jsonObject, name, "string", jsonArray);
        }

        private static void AddNumber(JsonObject jsonObject, string name)
        {
            Add(jsonObject, name, "number");
        }

        private static void AddBoolean(JsonObject jsonObject, string name)
        {
            Add(jsonObject, name, "boolean");
        }

        private static void Add(JsonObject jsonObject, string name, string type, JsonArray? jsonArray = null)
        {
            var description = GetDescriptionAttribute(name);
            Add(jsonObject, name, type, description, jsonArray);
        }
        private static void Add(JsonObject jsonObject, string name, string type, string description, JsonArray? jsonArray = null)
        {
            var property = new JsonObject
            {
                ["type"] = type,
                ["description"] = description
            };

            if (jsonArray != null)
            {
                property["enum"] = jsonArray;
            }
            jsonObject.Add(name, property);
        }
    }
}
