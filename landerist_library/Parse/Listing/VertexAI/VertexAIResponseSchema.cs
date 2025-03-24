using Google.Cloud.AIPlatform.V1;


namespace landerist_library.Parse.Listing.VertexAI
{
    public class VertexAIResponseSchema
    {
        public static OpenApiSchema Schema = new()
        {
            Type = Google.Cloud.AIPlatform.V1.Type.Object,
            Properties =
                {
                    ["es_un_anuncio"] = new()
                    {
                        Type = Google.Cloud.AIPlatform.V1.Type.Boolean,
                        Description = "Indica si el texto es un anuncio o no.",
                    },
                    ["anuncio"]= new()
                    {
                        Type = Google.Cloud.AIPlatform.V1.Type.Object,
                        Nullable = true,
                        Properties =
                        {
                            ["fecha_de_publicacion"] = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Format = "date",
                                Description = "Fecha de publicación del anuncio.",
                            },
                            ["tipo_de_operacion"] = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Enum = { "venta", "alquiler" },
                                Description = "Tipo de operación del anuncio.",
                            },
                            ["tipo_de_inmueble"] = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Enum = { "vivienda", "dormitorio", "local_comercial", "nave_industrial", "garaje", "trastero", "oficina", "parcela", "edificio" },
                                Description = "Tipo del inmueble anunciado.",
                            },
                            ["subtipo_de_inmueble"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Enum = { "piso", "apartamento", "ático", "bungalow", "duplex", "chalet_independiente", "chalet_pareado", "chalet_adosado", "parcela_urbana", "parcela_urbanizable", "parcela_no_urbanizable" },
                                Description = "Subtipo del inmueble anunciado.",
                            },
                            ["precio_del_anuncio"] = new()
                            {
                                Type = Google.Cloud.AIPlatform.V1.Type.Number,
                                Format = "decimal",
                                Description = "Precio del anuncio.",
                            },
                            ["descripcion_del_anuncio"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Description = "Descripción del anuncio.",
                            },
                            ["referencia_del_anuncio"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Description = "Referencia del anuncio.",
                            },
                            ["telefono_de_contacto"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Description = "Teléfono de contacto del anuncio",
                            },
                            ["email_de_contacto"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Format = "email",
                                Description = "Email de contacto del anuncio",
                            },
                            ["direccion_del_inmueble"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Description = "Dirección del inmueble",
                            },
                            ["referencia_catastral"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.String,
                                Description = "referencia catastral del anuncio (14 o 20 caracteres)",
                            },
                            ["urls_de_imagenes_del_anuncio"] = new()
                            {
                                Nullable = true,
                                Type = Google.Cloud.AIPlatform.V1.Type.Array,
                                Items = new()
                                {
                                    Type = Google.Cloud.AIPlatform.V1.Type.String,
                                    Format = "uri"
                                },
                                Description = "URLs de imágenes del anuncio.",
                            }
                        },
                        Required = { "fecha_de_publicacion", "tipo_de_operacion", "tipo_de_inmueble", "precio_del_anuncio" }
                    }
                },
            Required = { "es_un_anuncio" }
        };
    }
}

